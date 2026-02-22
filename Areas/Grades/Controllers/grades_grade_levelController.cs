using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    public class GradeLevelsController : Controller
    {
        private readonly AppDbContext _context;

        public GradeLevelsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: GradeLevels
        public async Task<IActionResult> Index()
        {
            var gradeLevels = await _context.grades_GradeLevels
                .Include(g => g.Groups)
                .Include(g => g.Subjects)
                .ToListAsync();
            return View(gradeLevels);
        }

        // GET: GradeLevels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var gradeLevel = await _context.grades_GradeLevels
                .Include(g => g.Groups)
                .Include(g => g.Subjects)
                .FirstOrDefaultAsync(m => m.GradeLevelId == id);

            if (gradeLevel == null) return NotFound();

            return View(gradeLevel);
        }

        // GET: GradeLevels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GradeLevels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(grades_grade_level gradeLevel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gradeLevel);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Nivel creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            return View(gradeLevel);
        }

        // GET: GradeLevels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var gradeLevel = await _context.grades_GradeLevels.FindAsync(id);
            if (gradeLevel == null) return NotFound();

            return View(gradeLevel);
        }

        // POST: GradeLevels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, grades_grade_level gradeLevel)
        {
            if (id != gradeLevel.GradeLevelId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gradeLevel);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Nivel actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GradeLevelExists(gradeLevel.GradeLevelId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(gradeLevel);
        }

        // POST: GradeLevels/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var gradeLevel = await _context.grades_GradeLevels.FindAsync(id);
            if (gradeLevel == null)
            {
                return NotFound();
            }

            // Verificar si tiene dependencias
            var hasGroups = await _context.grades_GradeGroups.AnyAsync(g => g.GradeLevelId == id);
            var hasSubjects = await _context.grades_Subjects.AnyAsync(s => s.GradeLevelId == id);

            if (hasGroups || hasSubjects)
            {
                TempData["Error"] = "No se puede eliminar porque tiene grupos o materias asociadas";
                return RedirectToAction(nameof(Index));
            }

            _context.grades_GradeLevels.Remove(gradeLevel);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Nivel eliminado exitosamente";

            return RedirectToAction(nameof(Index));
        }

        private bool GradeLevelExists(int id)
        {
            return _context.grades_GradeLevels.Any(e => e.GradeLevelId == id);
        }
    }
}