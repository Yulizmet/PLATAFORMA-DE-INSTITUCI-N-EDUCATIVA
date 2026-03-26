using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Grades.ViewModels.TeacherSubject;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    [Authorize(Roles = "Administrator")]

    public class TeacherSubjectController : Controller
    {
        private readonly AppDbContext _context;

        public TeacherSubjectController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TeacherSubject
        public async Task<IActionResult> Index(int? teacherId, int? subjectId)
        {
            var query = _context.grades_TeacherSubjects
                .Include(ts => ts.Teacher)
                    .ThenInclude(t => t.Person)
                .Include(ts => ts.Subject)
                    .ThenInclude(s => s.GradeLevel)
                .AsQueryable();

            if (teacherId.HasValue)
            {
                query = query.Where(ts => ts.TeacherId == teacherId.Value);
            }

            if (subjectId.HasValue)
            {
                query = query.Where(ts => ts.SubjectId == subjectId.Value);
            }

            var assignments = await query
                .Select(ts => new TeacherSubjectViewModel
                {
                    TeacherSubjectId = ts.TeacherSubjectId,
                    TeacherId = ts.TeacherId,
                    SubjectId = ts.SubjectId,
                    TeacherName = ts.Teacher.Person.FirstName + " " +
                                 ts.Teacher.Person.LastNamePaternal + " " +
                                 ts.Teacher.Person.LastNameMaternal,
                    SubjectName = ts.Subject.Name,
                    GradeLevelName = ts.Subject.GradeLevel.Name,
                    GroupsCount = _context.grades_TeacherSubjectGroups
                                    .Count(tsg => tsg.TeacherSubjectId == ts.TeacherSubjectId)
                })
                .OrderBy(ts => ts.GradeLevelName)
                .ThenBy(ts => ts.SubjectName)
                .ToListAsync();

            // Para los filtros
            ViewBag.Teachers = await GetTeachersAsync();
            ViewBag.Subjects = await _context.grades_Subjects
                .Include(s => s.GradeLevel)
                .OrderBy(s => s.GradeLevel.Name)
                .ThenBy(s => s.Name)
                .Select(s => new { s.SubjectId, Name = s.Name + " (" + s.GradeLevel.Name + ")" })
                .ToListAsync();

            ViewBag.SelectedTeacher = teacherId;
            ViewBag.SelectedSubject = subjectId;

            return View(assignments);
        }

        // GET: TeacherSubject/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Teachers = await GetTeachersAsync();
            ViewBag.Subjects = await _context.grades_Subjects
                .Include(s => s.GradeLevel)
                .OrderBy(s => s.GradeLevel.Name)
                .ThenBy(s => s.Name)
                .Select(s => new {
                    s.SubjectId,
                    DisplayName = s.Name + " (" + s.GradeLevel.Name + ")"
                })
                .ToListAsync();

            return View();
        }

        // POST: TeacherSubject/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherSubjectViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Verificar si ya existe la asignación
                var exists = await _context.grades_TeacherSubjects
                    .AnyAsync(ts => ts.TeacherId == viewModel.TeacherId
                                && ts.SubjectId == viewModel.SubjectId);

                if (exists)
                {
                    ModelState.AddModelError("", "Este profesor ya tiene asignada esta materia");
                }
                else
                {
                    var teacherSubject = new grades_teacher_subject
                    {
                        TeacherId = viewModel.TeacherId,
                        SubjectId = viewModel.SubjectId
                    };

                    _context.Add(teacherSubject);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Asignación creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Teachers = await GetTeachersAsync();
            ViewBag.Subjects = await _context.grades_Subjects
                .Include(s => s.GradeLevel)
                .OrderBy(s => s.GradeLevel.Name)
                .ThenBy(s => s.Name)
                .Select(s => new {
                    s.SubjectId,
                    DisplayName = s.Name + " (" + s.GradeLevel.Name + ")"
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: TeacherSubject/AssignGroups/5
        public async Task<IActionResult> AssignGroups(int? id)
        {
            if (id == null) return NotFound();

            var teacherSubject = await _context.grades_TeacherSubjects
                .Include(ts => ts.Teacher)
                    .ThenInclude(t => t.Person)
                .Include(ts => ts.Subject)
                    .ThenInclude(s => s.GradeLevel)
                .FirstOrDefaultAsync(ts => ts.TeacherSubjectId == id);

            if (teacherSubject == null) return NotFound();

            // Obtener todos los grupos del mismo nivel que la materia
            var allGroups = await _context.grades_GradeGroups
                .Where(g => g.GradeLevelId == teacherSubject.Subject.GradeLevelId)
                .OrderBy(g => g.Name)
                .ToListAsync();

            // Obtener los grupos ya asignados
            var assignedGroups = await _context.grades_TeacherSubjectGroups
                .Where(tsg => tsg.TeacherSubjectId == id)
                .ToListAsync();

            var viewModel = new TeacherAssignmentViewModel
            {
                TeacherSubjectId = teacherSubject.TeacherSubjectId,
                TeacherName = $"{teacherSubject.Teacher.Person.FirstName} {teacherSubject.Teacher.Person.LastNamePaternal}",
                SubjectName = teacherSubject.Subject.Name,
                GradeLevelName = teacherSubject.Subject.GradeLevel.Name,
                AvailableGroups = allGroups.Select(g => new GroupAssignmentViewModel
                {
                    GroupId = g.GroupId,
                    GroupName = g.Name,
                    IsAssigned = assignedGroups.Any(ag => ag.GroupId == g.GroupId),
                    TeacherSubjectGroupId = assignedGroups.FirstOrDefault(ag => ag.GroupId == g.GroupId)?.TeacherSubjectGroupId ?? 0
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: TeacherSubject/SaveGroupAssignments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGroupAssignments(int teacherSubjectId, List<int> selectedGroups)
        {
            var teacherSubject = await _context.grades_TeacherSubjects
                .FindAsync(teacherSubjectId);

            if (teacherSubject == null) return NotFound();

            // Obtener asignaciones actuales
            var currentAssignments = await _context.grades_TeacherSubjectGroups
                .Where(tsg => tsg.TeacherSubjectId == teacherSubjectId)
                .ToListAsync();

            // Eliminar las que ya no están seleccionadas
            var toRemove = currentAssignments
                .Where(ca => !selectedGroups.Contains(ca.GroupId))
                .ToList();

            _context.grades_TeacherSubjectGroups.RemoveRange(toRemove);

            // Agregar las nuevas
            var currentGroupIds = currentAssignments.Select(ca => ca.GroupId);
            var toAdd = selectedGroups
                .Where(sg => !currentGroupIds.Contains(sg))
                .Select(sg => new grades_teacher_subject_group
                {
                    TeacherSubjectId = teacherSubjectId,
                    GroupId = sg
                });

            await _context.grades_TeacherSubjectGroups.AddRangeAsync(toAdd);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Asignación de grupos actualizada exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // POST: TeacherSubject/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var teacherSubject = await _context.grades_TeacherSubjects
                .Include(ts => ts.TeacherSubjectGroups)
                .FirstOrDefaultAsync(ts => ts.TeacherSubjectId == id);

            if (teacherSubject == null) return NotFound();

            // Verificar si tiene grupos asignados
            if (teacherSubject.TeacherSubjectGroups.Any())
            {
                _context.grades_TeacherSubjectGroups.RemoveRange(teacherSubject.TeacherSubjectGroups);
            }

            _context.grades_TeacherSubjects.Remove(teacherSubject);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Asignación eliminada exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // Helper para obtener solo usuarios con rol de profesor
        private async Task<List<dynamic>> GetTeachersAsync()
        {
            // Buscar el rol "Profesor" (ajusta el nombre según tu base de datos)
            var teacherRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == "profesor"
                                       || r.Name.ToLower() == "teacher"
                                       || r.Name.ToLower() == "docente");

            if (teacherRole == null)
            {
                // Temporal: traer todos los usuarios activos
                return await _context.Users
                    .Include(u => u.Person)
                    .Where(u => u.IsActive)
                    .Select(u => new {
                        u.UserId,
                        FullName = u.Person.FirstName + " " +
                                  u.Person.LastNamePaternal + " " +
                                  u.Person.LastNameMaternal
                    })
                    .OrderBy(u => u.FullName)
                    .ToListAsync<dynamic>();
            }

            // Traer usuarios con el rol de profesor
            return await _context.UserRoles
                .Include(ur => ur.User)
                    .ThenInclude(u => u!.Person)
                .Where(ur => ur.RoleId == teacherRole.RoleId && ur.IsActive)
                .Select(ur => new {
                    ur!.User!.UserId,
                    FullName = ur.User.Person.FirstName + " " +
                              ur.User.Person.LastNamePaternal + " " +
                              ur.User.Person.LastNameMaternal
                })
                .OrderBy(u => u.FullName)
                .ToListAsync<dynamic>();
        }
    }
}