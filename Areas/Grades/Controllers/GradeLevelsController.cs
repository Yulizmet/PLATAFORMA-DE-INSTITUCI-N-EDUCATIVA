using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Grades.ViewModels;
using SchoolManager.Areas.Grades.ViewModels.GradeLevels;
using SchoolManager.Data;
using SchoolManager.Models;


namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    [Authorize(Roles = "Administrator")]

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
                .Select(g => new GradeLevelViewModel
                {
                    GradeLevelId = g.GradeLevelId,
                    Name = g.Name,
                    StartDate = g.StartDate,
                    EndDate = g.EndDate,
                    IsOpen = g.IsOpen,
                    MinPassingGrade = g.MinPassingGrade,
                    GroupsCount = g.Groups.Count,
                    SubjectsCount = g.Subjects.Count
                })
                .OrderByDescending(g => g.IsOpen)
                .ThenBy(g => g.StartDate)
                .ToListAsync();

            return View(gradeLevels);
        }        // GET: GradeLevels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gradeLevel = await _context.grades_GradeLevels
                .Where(gl => gl.GradeLevelId == id)
                .Select(gl => new GradeLevelDetailsViewModel
                {
                    GradeLevelId = gl.GradeLevelId,
                    Name = gl.Name,
                    StartDate = gl.StartDate,
                    EndDate = gl.EndDate,
                    IsOpen = gl.IsOpen,
                    Groups = gl.Groups.Select(g => new GroupSimpleViewModel
                    {
                        GroupId = g.GroupId,
                        Name = g.Name,
                        SubjectsCount = g.TeacherSubjectGroups.Count
                    }).ToList(),
                    Subjects = gl.Subjects.Select(s => new SubjectSimpleViewModel
                    {
                        SubjectId = s.SubjectId,
                        Name = s.Name,
                        UnitsCount = s.Units.Count
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (gradeLevel == null)
            {
                return NotFound();
            }

            return View(gradeLevel);
        }

        // GET: GradeLevels/Create
        public IActionResult Create()
        {
            var viewModel = new GradeLevelViewModel
            {
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
                IsOpen = true,
                MinPassingGrade = 6.0m
            };

            return View(viewModel);
        }

        // POST: GradeLevels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GradeLevelViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var gradeLevel = new grades_grade_level
                {
                    Name = viewModel.Name,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    IsOpen = viewModel.IsOpen
                };

                _context.Add(gradeLevel);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Nivel creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: GradeLevels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gradeLevel = await _context.grades_GradeLevels
                .Where(gl => gl.GradeLevelId == id)
                .Select(gl => new GradeLevelViewModel
                {
                    GradeLevelId = gl.GradeLevelId,
                    Name = gl.Name,
                    StartDate = gl.StartDate,
                    EndDate = gl.EndDate,
                    IsOpen = gl.IsOpen,
                    MinPassingGrade = gl.MinPassingGrade
                })
                .FirstOrDefaultAsync();

            if (gradeLevel == null)
            {
                return NotFound();
            }

            return View(gradeLevel);
        }

        // POST: GradeLevels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GradeLevelViewModel viewModel)
        {
            if (id != viewModel.GradeLevelId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var gradeLevel = await _context.grades_GradeLevels.FindAsync(id);
                    if (gradeLevel == null)
                    {
                        return NotFound();
                    }

                    gradeLevel.Name = viewModel.Name;
                    gradeLevel.StartDate = viewModel.StartDate;
                    gradeLevel.EndDate = viewModel.EndDate;
                    gradeLevel.IsOpen = viewModel.IsOpen;
                    gradeLevel.MinPassingGrade = viewModel.MinPassingGrade;

                    _context.Update(gradeLevel);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Nivel actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GradeLevelExists(viewModel.GradeLevelId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // POST: GradeLevels/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var gradeLevel = await _context.grades_GradeLevels
                .Include(gl => gl.Groups)
                .Include(gl => gl.Subjects)
                .FirstOrDefaultAsync(gl => gl.GradeLevelId == id);

            if (gradeLevel == null)
            {
                return NotFound();
            }

            // Verificar si tiene dependencias
            if (gradeLevel.Groups.Any() || gradeLevel.Subjects.Any())
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