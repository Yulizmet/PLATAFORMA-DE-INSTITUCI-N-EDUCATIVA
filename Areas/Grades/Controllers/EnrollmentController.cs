using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Grades.ViewModels.Enrollment;
using SchoolManager.Areas.MainScreen.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    public class EnrollmentController : Controller
    {
        private readonly AppDbContext _context;

        public EnrollmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Enrollment/ByGroup/5
        // GET: Enrollment/ByGroup/5
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

            var students = group.Enrollments.Select(e => new EnrollmentViewModel
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                StudentName = $"{e.Student.Person.FirstName} {e.Student.Person.LastNamePaternal} {e.Student.Person.LastNameMaternal}",
                Matricula = e.Student.Person.Curp ?? "S/N",
                //Email = e.Student.Email ?? "S/N",
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

            // Obtener estudiantes ya inscritos en este grupo
            var enrolledStudentIds = group.Enrollments.Select(e => e.StudentId).ToList();

            // Obtener todos los usuarios con rol de estudiante (de momento sin rol)
            var students = await _context.UserRoles
                .Include(ur => ur.User)
                    .ThenInclude(u => u.Person)
                //.Where(ur => ur.RoleId == 3 && ur.IsActive) // Rol de estudiante (sin validacion ahorita)
                .Select(ur => new StudentSelectionViewModel
                {
                    StudentId = ur.User.UserId,
                    FullName = ur.User.Person.FirstName + " " +
                               ur.User.Person.LastNamePaternal + " " +
                               ur.User.Person.LastNameMaternal,
                    Matricula = ur.User.Person.Curp, // O donde tengas matrícula
                    Email = ur.User.Email,
                    IsSelected = enrolledStudentIds.Contains(ur.User.UserId)
                })
                .ToListAsync();

            var viewModel = new EnrollmentCreateViewModel
            {
                GroupId = group.GroupId,
                GroupName = group.Name,
                GradeLevelName = group.GradeLevel.Name,
                AvailableStudents = students
            };

            return View(viewModel);
        }

        // POST: Enrollment/SaveAssignments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAssignments(int groupId, List<int> selectedStudentIds)
        {
            var group = await _context.grades_GradeGroups
                .Include(g => g.Enrollments)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null) return NotFound();

            // Eliminar inscripciones que ya no están seleccionadas
            var toRemove = group.Enrollments
                .Where(e => !selectedStudentIds.Contains(e.StudentId))
                .ToList();

            _context.grades_Enrollments.RemoveRange(toRemove);

            // Agregar nuevas inscripciones
            var currentStudentIds = group.Enrollments.Select(e => e.StudentId);
            var toAdd = selectedStudentIds
                .Where(id => !currentStudentIds.Contains(id))
                .Select(id => new grades_enrollment
                {
                    StudentId = id,
                    GroupId = groupId
                });

            await _context.grades_Enrollments.AddRangeAsync(toAdd);
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

            _context.grades_Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Estudiante removido del grupo";
            return RedirectToAction(nameof(ByGroup), new { groupId });
        }
    }
}