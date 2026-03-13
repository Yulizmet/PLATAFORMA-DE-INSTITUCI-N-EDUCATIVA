using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Grades.ViewModels.Subjects;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.Areas.Grades.ViewModels;

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
                .Select(s => new SubjectViewModel
                {
                    SubjectId = s.SubjectId,
                    Name = s.Name,
                    GradeLevelId = s.GradeLevelId,
                    GradeLevelName = s.GradeLevel.Name,
                    UnitsCount = s.Units.Count,
                    OpenUnitsCount = s.Units.Count(u => u.IsOpen)
                })
                .OrderBy(s => s.GradeLevelName)
                .ThenBy(s => s.Name)
                .ToListAsync();

            ViewBag.GradeLevels = await _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name, gl.IsOpen })
                .ToListAsync();

            ViewBag.SelectedGradeLevel = gradeLevelId;

            return View(subjects);
        }

        // GET: Subjects/Create
        public IActionResult Create(int? gradeLevelId)
        {
            var viewModel = new SubjectViewModel();

            if (gradeLevelId.HasValue)
            {
                viewModel.GradeLevelId = gradeLevelId.Value;
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(viewModel);
        }

        // POST: Subjects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var subject = new grades_subjects
                {
                    Name = viewModel.Name,
                    GradeLevelId = viewModel.GradeLevelId
                };

                _context.Add(subject);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Materia creada exitosamente";
                return RedirectToAction(nameof(Index), new { gradeLevelId = viewModel.GradeLevelId });
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(viewModel);
        }

        // GET: Subjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subject = await _context.grades_Subjects
                .Where(s => s.SubjectId == id)
                .Select(s => new SubjectViewModel
                {
                    SubjectId = s.SubjectId,
                    Name = s.Name,
                    GradeLevelId = s.GradeLevelId
                })
                .FirstOrDefaultAsync();

            if (subject == null) return NotFound();

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(subject);
        }

        // POST: Subjects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubjectViewModel viewModel)
        {
            if (id != viewModel.SubjectId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var subject = await _context.grades_Subjects.FindAsync(id);
                    if (subject == null) return NotFound();

                    subject.Name = viewModel.Name;
                    subject.GradeLevelId = viewModel.GradeLevelId;

                    _context.Update(subject);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Materia actualizada exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(viewModel.SubjectId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index), new { gradeLevelId = viewModel.GradeLevelId });
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(viewModel);
        }

        // GET: Subjects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var subject = await _context.grades_Subjects
                .Where(s => s.SubjectId == id)
                .Select(s => new SubjectDetailsViewModel
                {
                    SubjectId = s.SubjectId,
                    Name = s.Name,
                    GradeLevelId = s.GradeLevelId,
                    GradeLevelName = s.GradeLevel.Name,
                    Units = s.Units
                        .OrderBy(u => u.UnitNumber)
                        .Select(u => new UnitViewModel
                        {
                            UnitId = u.UnitId,
                            UnitNumber = u.UnitNumber,
                            IsOpen = u.IsOpen,
                            HasGrades = _context.grades_Grades.Any(g => g.SubjectUnitId == u.UnitId)
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (subject == null) return NotFound();

            return View(subject);
        }

        // GET: Subjects/ManageUnits/5
        public async Task<IActionResult> ManageUnits(int? subjectId)
        {
            if (subjectId == null) return NotFound();

            var subject = await _context.grades_Subjects
                .Include(s => s.GradeLevel)
                .Include(s => s.Units)
                .FirstOrDefaultAsync(s => s.SubjectId == subjectId);

            if (subject == null) return NotFound();

            var viewModel = new ManageUnitsViewModel
            {
                SubjectId = subject.SubjectId,
                SubjectName = subject.Name,
                GradeLevelName = subject.GradeLevel.Name,
                Units = subject.Units
                    .OrderBy(u => u.UnitNumber)
                    .Select(u => new UnitViewModel
                    {
                        UnitId = u.UnitId,
                        UnitNumber = u.UnitNumber,
                        IsOpen = u.IsOpen,
                        HasGrades = _context.grades_Grades.Any(g => g.SubjectUnitId == u.UnitId)
                    }).ToList()
            };

            return View(viewModel);
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

            // Eliminar primero las unidades
            if (subject.Units.Any())
            {
                _context.grades_SubjectUnits.RemoveRange(subject.Units);
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