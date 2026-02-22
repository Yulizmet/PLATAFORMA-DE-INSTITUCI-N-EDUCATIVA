using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    public class GroupsController : Controller
    {
        private readonly AppDbContext _context;

        public GroupsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Groups
        public async Task<IActionResult> Index(int? gradeLevelId)
        {
            var query = _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .AsQueryable();

            if (gradeLevelId.HasValue)
            {
                query = query.Where(g => g.GradeLevelId == gradeLevelId.Value);
            }

            var groups = await query
                .OrderBy(g => g.GradeLevel.Name)
                .ThenBy(g => g.Name)
                .ToListAsync();

            ViewBag.GradeLevels = await _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .ToListAsync();

            ViewBag.SelectedGradeLevel = gradeLevelId;

            return View(groups);
        }

        // GET: Groups/Create
        public IActionResult Create(int? gradeLevelId)
        {
            ViewBag.GradeLevels = _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .ToList();

            if (gradeLevelId.HasValue)
            {
                ViewBag.SelectedGradeLevel = gradeLevelId.Value;
            }

            return View();
        }

        // POST: Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(grades_group group)
        {
            // Limpiar validación de la propiedad de navegación
            ModelState.Remove("GradeLevel");

            if (ModelState.IsValid)
            {
                _context.Add(group);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Grupo creado exitosamente";
                return RedirectToAction(nameof(Index), new { gradeLevelId = group.GradeLevelId });
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .ToList();

            return View(group);
        }

        // GET: Groups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var group = await _context.grades_GradeGroups.FindAsync(id);
            if (group == null) return NotFound();

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .ToList();

            return View(group);
        }

        // POST: Groups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, grades_group group)
        {
            if (id != group.GroupId) return NotFound();

            ModelState.Remove("GradeLevel");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(group);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Grupo actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(group.GroupId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index), new { gradeLevelId = group.GradeLevelId });
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .ToList();

            return View(group);
        }

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var group = await _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .Include(g => g.TeacherSubjectGroups)
                    .ThenInclude(tsg => tsg.TeacherSubject)
                        .ThenInclude(ts => ts.Subject)
                .Include(g => g.TeacherSubjectGroups)
                    .ThenInclude(tsg => tsg.TeacherSubject)
                        .ThenInclude(ts => ts.Teacher)
                            .ThenInclude(t => t.Person)
                .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null) return NotFound();

            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.grades_GradeGroups
                .Include(g => g.TeacherSubjectGroups)
                .Include(g => g.FinalGrades)
                .Include(g => g.Grades)
                .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null) return NotFound();

            // Verificar si tiene dependencias
            if (group.TeacherSubjectGroups.Any() || group.FinalGrades.Any() || group.Grades.Any())
            {
                TempData["Error"] = "No se puede eliminar el grupo porque tiene profesores asignados o calificaciones registradas";
                return RedirectToAction(nameof(Index));
            }

            _context.grades_GradeGroups.Remove(group);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Grupo eliminado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(int id)
        {
            return _context.grades_GradeGroups.Any(e => e.GroupId == id);
        }
    }
}