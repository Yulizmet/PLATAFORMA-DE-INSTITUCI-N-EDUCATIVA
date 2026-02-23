using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.Areas.Grades.ViewModels.Groups;

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
                .Include(g => g.TeacherSubjectGroups)
                .AsQueryable();

            if (gradeLevelId.HasValue)
            {
                query = query.Where(g => g.GradeLevelId == gradeLevelId.Value);
            }

            var groups = await query
                .Select(g => new GroupViewModel
                {
                    GroupId = g.GroupId,
                    Name = g.Name,
                    GradeLevelId = g.GradeLevelId,
                    GradeLevelName = g.GradeLevel.Name,
                    SubjectsCount = g.TeacherSubjectGroups.Count,
                    StudentsCount = 0 // Por ahora, hasta que tengamos inscripciones
                })
                .OrderBy(g => g.GradeLevelName)
                .ThenBy(g => g.Name)
                .ToListAsync();

            ViewBag.GradeLevels = await _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToListAsync();

            ViewBag.SelectedGradeLevel = gradeLevelId;

            return View(groups);
        }

        // GET: Groups/Create
        public IActionResult Create(int? gradeLevelId)
        {
            var viewModel = new GroupViewModel();

            if (gradeLevelId.HasValue)
            {
                viewModel.GradeLevelId = gradeLevelId.Value;
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(viewModel);
        }

        // POST: Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var group = new grades_group
                {
                    Name = viewModel.Name,
                    GradeLevelId = viewModel.GradeLevelId
                };

                _context.Add(group);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Grupo creado exitosamente";
                return RedirectToAction(nameof(Index), new { gradeLevelId = viewModel.GradeLevelId });
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(viewModel);
        }

        // GET: Groups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var group = await _context.grades_GradeGroups
                .Where(g => g.GroupId == id)
                .Select(g => new GroupViewModel
                {
                    GroupId = g.GroupId,
                    Name = g.Name,
                    GradeLevelId = g.GradeLevelId
                })
                .FirstOrDefaultAsync();

            if (group == null) return NotFound();

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(group);
        }

        // POST: Groups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GroupViewModel viewModel)
        {
            if (id != viewModel.GroupId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var group = await _context.grades_GradeGroups.FindAsync(id);
                    if (group == null) return NotFound();

                    group.Name = viewModel.Name;
                    group.GradeLevelId = viewModel.GradeLevelId;

                    _context.Update(group);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Grupo actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(viewModel.GroupId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index), new { gradeLevelId = viewModel.GradeLevelId });
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(viewModel);
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
                .Include(g => g.Enrollments)  
                    .ThenInclude(e => e.Student)
                        .ThenInclude(s => s.Person)
                .Where(g => g.GroupId == id)
                .Select(g => new GroupDetailsViewModel
                {
                    GroupId = g.GroupId,
                    Name = g.Name,
                    GradeLevelId = g.GradeLevelId,
                    GradeLevelName = g.GradeLevel.Name,
                    Subjects = g.TeacherSubjectGroups.Select(tsg => new GroupSubjectViewModel
                    {
                        TeacherSubjectGroupId = tsg.TeacherSubjectGroupId,
                        SubjectName = tsg.TeacherSubject.Subject.Name,
                        TeacherName = tsg.TeacherSubject.Teacher.Person.FirstName + " " +
                                     tsg.TeacherSubject.Teacher.Person.LastNamePaternal
                    }).ToList(),
                    StudentsCount = g.Enrollments.Count,
                    Students = g.Enrollments.Select(e => new GroupStudentViewModel
                    {
                        EnrollmentId = e.EnrollmentId,
                        StudentId = e.StudentId,
                        FullName = e.Student.Person.FirstName + " " +
                                  e.Student.Person.LastNamePaternal + " " +
                                  e.Student.Person.LastNameMaternal,
                        Matricula = e.Student.Person.Curp, //Matricula no se donde esta aun
                        Email = e.Student.Email
                    }).ToList()
                })
                .FirstOrDefaultAsync();

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