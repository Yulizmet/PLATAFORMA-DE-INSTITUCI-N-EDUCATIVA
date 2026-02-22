using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    public class SubjectsController : Controller
    {
        private readonly AppDbContext _context;

        public SubjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Subjects
        public async Task<IActionResult> Index(int? gradeLevelId)
        {
            var query = _context.grades_Subjects
                .Include(s => s.GradeLevel)
                .Include(s => s.Units)
                .AsQueryable();

            if (gradeLevelId.HasValue)
            {
                query = query.Where(s => s.GradeLevelId == gradeLevelId.Value);
            }

            var subjects = await query
                .OrderBy(s => s.GradeLevel.Name)
                .ThenBy(s => s.Name)
                .ToListAsync();

            ViewBag.GradeLevels = await _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .ToListAsync();

            ViewBag.SelectedGradeLevel = gradeLevelId;

            return View(subjects);
        }

        // GET: Subjects/Create
        public IActionResult Create(int? gradeLevelId)
        {
            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .ToList();

            if (gradeLevelId.HasValue)
            {
                ViewBag.SelectedGradeLevel = gradeLevelId.Value;
            }

            return View();
        }

        // POST: Subjects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(grades_subjects subject)
        {
            ModelState.Remove("GradeLevel");
            if (ModelState.IsValid)
            {
                _context.Add(subject);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Materia creada exitosamente";
                return RedirectToAction(nameof(Index), new { gradeLevelId = subject.GradeLevelId });
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .ToList();

            return View(subject);
        }

        // GET: Subjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subject = await _context.grades_Subjects.FindAsync(id);
            if (subject == null) return NotFound();

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .ToList();

            return View(subject);
        }

        // POST: Subjects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, grades_subjects subject)
        {
            if (id != subject.SubjectId) return NotFound();

            // Limpiar validación de la propiedad de navegación
            ModelState.Remove("GradeLevel");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subject);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Materia actualizada exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(subject.SubjectId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index), new { gradeLevelId = subject.GradeLevelId });
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .ToList();

            return View(subject);
        }

        // GET: Subjects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var subject = await _context.grades_Subjects
                .Include(s => s.GradeLevel)
                .Include(s => s.Units.OrderBy(u => u.UnitNumber))
                .FirstOrDefaultAsync(m => m.SubjectId == id);

            if (subject == null) return NotFound();

            return View(subject);
        }

        // GET: Subjects/ManageUnits/5
        public async Task<IActionResult> ManageUnits(int? subjectId)
        {
            if (subjectId == null) return NotFound();

            var subject = await _context.grades_Subjects
                .Include(s => s.Units.OrderBy(u => u.UnitNumber))
                .FirstOrDefaultAsync(s => s.SubjectId == subjectId);

            if (subject == null) return NotFound();

            return View(subject);
        }

        // POST: Subjects/AddUnit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnit(int subjectId, int unitNumber)
        {
            var subject = await _context.grades_Subjects
                .Include(s => s.Units)
                .FirstOrDefaultAsync(s => s.SubjectId == subjectId);

            if (subject == null) return NotFound();

            // Verificar si ya existe una unidad con ese número
            if (subject.Units.Any(u => u.UnitNumber == unitNumber))
            {
                TempData["Error"] = $"La unidad {unitNumber} ya existe";
                return RedirectToAction(nameof(ManageUnits), new { subjectId });
            }

            var unit = new grades_subject_unit
            {
                SubjectId = subjectId,
                UnitNumber = unitNumber,
                IsOpen = true
            };

            _context.grades_SubjectUnits.Add(unit);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Unidad {unitNumber} agregada exitosamente";
            return RedirectToAction(nameof(ManageUnits), new { subjectId });
        }

        // POST: Subjects/ToggleUnit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUnit(int unitId)
        {
            var unit = await _context.grades_SubjectUnits.FindAsync(unitId);
            if (unit == null) return NotFound();

            unit.IsOpen = !unit.IsOpen;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Unidad {(unit.IsOpen ? "abierta" : "cerrada")} exitosamente";
            return RedirectToAction(nameof(ManageUnits), new { subjectId = unit.SubjectId });
        }

        // POST: Subjects/DeleteUnit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnit(int unitId)
        {
            var unit = await _context.grades_SubjectUnits
                .Include(u => u.Subject)
                .FirstOrDefaultAsync(u => u.UnitId == unitId);

            if (unit == null) return NotFound();

            // Verificar si tiene calificaciones asociadas
            var hasGrades = await _context.grades_Grades.AnyAsync(g => g.SubjectUnitId == unitId);
            if (hasGrades)
            {
                TempData["Error"] = "No se puede eliminar la unidad porque tiene calificaciones asociadas";
                return RedirectToAction(nameof(ManageUnits), new { subjectId = unit.SubjectId });
            }

            _context.grades_SubjectUnits.Remove(unit);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Unidad eliminada exitosamente";
            return RedirectToAction(nameof(ManageUnits), new { subjectId = unit.SubjectId });
        }

        // POST: Subjects/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _context.grades_Subjects
                .Include(s => s.Units)
                .FirstOrDefaultAsync(s => s.SubjectId == id);

            if (subject == null) return NotFound();

            // Verificar si tiene unidades con calificaciones
            var hasGrades = await _context.grades_Grades
                .AnyAsync(g => subject.Units.Select(u => u.UnitId).Contains(g.SubjectUnitId));

            if (hasGrades)
            {
                TempData["Error"] = "No se puede eliminar la materia porque tiene calificaciones asociadas";
                return RedirectToAction(nameof(Index));
            }

            _context.grades_Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Materia eliminada exitosamente";
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectExists(int id)
        {
            return _context.grades_Subjects.Any(e => e.SubjectId == id);
        }
    }
}