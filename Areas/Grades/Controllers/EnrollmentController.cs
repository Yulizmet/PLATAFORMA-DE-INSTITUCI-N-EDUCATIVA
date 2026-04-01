using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Grades.ViewModels.Enrollment;
using SchoolManager.Areas.MainScreen.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    [Authorize(Roles = "Administrator")]

    public class EnrollmentController : Controller
    {
        private readonly AppDbContext _context;

        public EnrollmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Enrollment/ByGroup/5
        public async Task<IActionResult> ByGroup(int? groupId)
        {
            if (groupId == null) return NotFound();

            var group = await _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .Include(g => g.Enrollments)
                    .ThenInclude(e => e.Student)
                        .ThenInclude(s => s.Person)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null) return NotFound();

            ViewBag.GroupId = group.GroupId;
            ViewBag.GroupName = group.Name;
            ViewBag.GradeLevelName = group.GradeLevel.Name;

            var studentIds = group.Enrollments.Select(e => e.StudentId).ToList();
            var matriculas = await _context.Set<preenrollment_general>()
                .Where(p => p.UserId != null && studentIds.Contains(p.UserId.Value))
                .GroupBy(p => p.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Matricula = g.OrderByDescending(p => p.CreateStat).Select(p => p.Matricula).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.UserId!.Value, x => x.Matricula ?? "S/N");

            var students = group.Enrollments
                .Where(e => e.IsActive)
                .Select(e => new EnrollmentViewModel
                {
                    EnrollmentId = e.EnrollmentId,
                    StudentId = e.StudentId,
                    StudentName = $"{e.Student.Person.FirstName} {e.Student.Person.LastNamePaternal} {e.Student.Person.LastNameMaternal}",
                    Matricula = matriculas.GetValueOrDefault(e.StudentId, "S/N"),
                    GroupId = e.GroupId,
                    GroupName = group.Name,
                    GradeLevelName = group.GradeLevel.Name
                }).ToList();

            return View(students);
        }

        // GET: Enrollment/SelectGroupForAssignment
        public async Task<IActionResult> SelectGroupForAssignment()
        {
            var grupos = await _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .Where(g => g.GradeLevel.IsOpen)
                .OrderBy(g => g.GradeLevel.Name)
                .ThenBy(g => g.Name)
                .Select(g => new GrupoResumenViewModel
                {
                    GroupId = g.GroupId,
                    Name = g.Name,
                    GradeLevelName = g.GradeLevel.Name
                })
                .ToListAsync();

            ViewBag.Accion = "AssignToGroup";
            ViewBag.Titulo = "Seleccionar Grupo para Inscribir Estudiantes";
            ViewBag.Boton = "Continuar con Inscripción";

            return View("SelectGroup", grupos);
        }

        // GET: Enrollment/SelectGroupForViewing
        public async Task<IActionResult> SelectGroupForViewing()
        {
            var grupos = await _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .OrderBy(g => g.GradeLevel.Name)
                .ThenBy(g => g.Name)
                .Select(g => new GrupoResumenViewModel
                {
                    GroupId = g.GroupId,
                    Name = g.Name,
                    GradeLevelName = g.GradeLevel.Name
                })
                .ToListAsync();

            ViewBag.Accion = "ByGroup";
            ViewBag.Titulo = "Seleccionar Grupo para Ver Estudiantes";
            ViewBag.Boton = "Ver Estudiantes";

            return View("SelectGroup", grupos);
        }

        // GET: Enrollment/AssignToGroup/5
        public async Task<IActionResult> AssignToGroup(int? groupId)
        {
            if (groupId == null) return NotFound();

            var group = await _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .Include(g => g.Enrollments)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null) return NotFound();

            // IDs de alumnos activos en ESTE grupo
            var enrolledStudentIds = group.Enrollments
                .Where(e => e.IsActive)
                .Select(e => e.StudentId)
                .ToList();

            // Todos los enrollments activos en OTROS grupos (para saber dónde está cada alumno)
            var otherActiveEnrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => e.IsActive && e.GroupId != groupId)
                .ToListAsync();

            // Datos de semestres y grupos para los filtros de la vista
            var gradeLevels = await _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToListAsync();

            var allGroups = await _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .OrderBy(g => g.GradeLevel.Name)
                .ThenBy(g => g.Name)
                .Select(g => new { g.GroupId, g.Name, g.GradeLevelId, GradeLevelName = g.GradeLevel.Name })
                .ToListAsync();

            // Obtener usuarios con rol Student
            var studentRoleUsers = await _context.UserRoles
                .Include(ur => ur.User)
                    .ThenInclude(u => u.Person)
                .Where(ur => ur.Role.Name == "Student" && ur.IsActive)
                .Select(ur => new
                {
                    ur.User.UserId,
                    ur.User.Email,
                    ur.User.Person.FirstName,
                    ur.User.Person.LastNamePaternal,
                    ur.User.Person.LastNameMaternal,
                    ur.User.Person.Curp
                })
                .ToListAsync();

            var students = studentRoleUsers.Select(u =>
            {
                var otherEnrollment = otherActiveEnrollments
                    .FirstOrDefault(e => e.StudentId == u.UserId);

                return new StudentSelectionViewModel
                {
                    StudentId = u.UserId,
                    FullName = $"{u.FirstName} {u.LastNamePaternal} {u.LastNameMaternal}",
                    Matricula = u.Curp,
                    Email = u.Email,
                    IsSelected = enrolledStudentIds.Contains(u.UserId),
                    CurrentGroupName = otherEnrollment?.Group.Name,
                    CurrentGradeLevelName = otherEnrollment?.Group.GradeLevel.Name
                };
            }).ToList();

            var viewModel = new EnrollmentCreateViewModel
            {
                GroupId = group.GroupId,
                GroupName = group.Name,
                GradeLevelName = group.GradeLevel.Name,
                AvailableStudents = students
            };

            ViewBag.GradeLevels = gradeLevels;
            ViewBag.AllGroups = allGroups;

            return View(viewModel);
        }

        // POST: Enrollment/SaveAssignments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAssignments(int groupId, List<int> selectedStudentIds)
        {
            selectedStudentIds ??= new List<int>();

            var group = await _context.grades_GradeGroups
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null) return NotFound();

            // Enrollments activos en este grupo
            var currentEnrollments = await _context.grades_Enrollments
                .Where(e => e.GroupId == groupId && e.IsActive)
                .ToListAsync();

            // Desactivar los que ya no están seleccionados (en lugar de borrar)
            foreach (var enrollment in currentEnrollments)
            {
                if (!selectedStudentIds.Contains(enrollment.StudentId))
                {
                    enrollment.IsActive = false;
                }
            }

            // Para los nuevos seleccionados
            var currentActiveIds = currentEnrollments
                .Where(e => e.IsActive)
                .Select(e => e.StudentId)
                .ToList();

            foreach (var studentId in selectedStudentIds.Where(id => !currentActiveIds.Contains(id)))
            {
                // Buscar si existe un enrollment inactivo previo para reactivar
                var existing = await _context.grades_Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && e.GroupId == groupId && !e.IsActive);

                if (existing != null)
                {
                    existing.IsActive = true;
                    existing.EnrolledAt = DateTime.Now;
                }
                else
                {
                    // Desactivar cualquier enrollment activo en OTRO grupo
                    var otherEnrollments = await _context.grades_Enrollments
                        .Where(e => e.StudentId == studentId && e.GroupId != groupId && e.IsActive)
                        .ToListAsync();

                    foreach (var other in otherEnrollments)
                        other.IsActive = false;

                    _context.grades_Enrollments.Add(new grades_enrollment
                    {
                        StudentId = studentId,
                        GroupId = groupId,
                        IsActive = true,
                        EnrolledAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Estudiantes asignados exitosamente";
            return RedirectToAction(nameof(ByGroup), new { groupId });
        }

        // POST: Enrollment/Remove/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int enrollmentId)
        {
            var enrollment = await _context.grades_Enrollments
                .Include(e => e.Group)
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

            if (enrollment == null) return NotFound();

            var groupId = enrollment.GroupId;

            // Desactivar en lugar de borrar
            enrollment.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Estudiante removido del grupo";
            return RedirectToAction(nameof(ByGroup), new { groupId });
        }
    }
}