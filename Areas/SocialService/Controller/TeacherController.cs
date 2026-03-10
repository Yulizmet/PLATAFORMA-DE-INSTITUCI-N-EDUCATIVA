using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.Areas.SocialService.ViewModels;

namespace SchoolManager.Areas.SocialService.Controllers
{
    [Area("SocialService")]
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;

        public TeacherController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Dashboard()
        {
            int currentTeacherId = GetCurrentTeacherId();

            if (currentTeacherId == 0)
            {
                TempData["Error"] = "No se pudo identificar al usuario.";
                return RedirectToAction("Login", "Account", new { area = "UserMng" });
            }

            DateTime today = DateTime.Today;

            var alumnosAsignados = await _context.SocialServiceAssignments
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .CountAsync();

            var studentIds = await _context.SocialServiceAssignments
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .Select(a => a.StudentId)
                .ToListAsync();

            var bitacorasPendientes = await _context.SocialServiceLogs
                .Where(log => studentIds.Contains(log.StudentId) && !log.IsApproved)
                .CountAsync();

            var asistenciasHoy = await _context.SocialServiceAttendances
                .Where(att => studentIds.Contains(att.StudentId)
                    && att.Date.Date == today
                    && att.IsPresent)
                .CountAsync();

            ViewBag.AlumnosAsignados = alumnosAsignados;
            ViewBag.BitacorasPendientes = bitacorasPendientes;
            ViewBag.AsistenciasHoy = asistenciasHoy;

            return View();
        }

        public async Task<IActionResult> Alumnos()
        {
            int currentTeacherId = GetCurrentTeacherId();

            if (currentTeacherId == 0)
            {
                TempData["Error"] = "No se pudo identificar al usuario.";
                return RedirectToAction("Login", "Account", new { area = "UserMng" });
            }

            var assignedStudents = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Include(a => a.Student.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .ToListAsync();

            // Enriquecer con información de semestre y grupo
            foreach (var assignment in assignedStudents)
            {
                var enrollment = await _context.grades_Enrollments
                    .Include(e => e.Group)
                        .ThenInclude(g => g.GradeLevel)
                    .Where(e => e.StudentId == assignment.StudentId)
                    .FirstOrDefaultAsync();

                if (enrollment != null)
                {
                    assignment.SemesterName = enrollment.Group?.GradeLevel?.Name;
                    assignment.GroupName = enrollment.Group?.Name;
                }
            }

            return View(assignedStudents);
        }

        public async Task<IActionResult> AgregarAlumnos()
        {
            int currentTeacherId = GetCurrentTeacherId();

            // Excluir alumnos asignados activamente a cualquier maestro
            var assignedStudentIds = await _context.SocialServiceAssignments
                .Where(a => a.IsActive)
                .Select(a => a.StudentId)
                .ToListAsync();

            var availableStudents = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student" && ur.IsActive)
                    && !assignedStudentIds.Contains(u.UserId)
                    && u.IsActive)
                .OrderBy(u => u.Person.FirstName)
                    .ThenBy(u => u.Person.LastNamePaternal)
                .ToListAsync();

            var viewModel = new List<AvailableStudentViewModel>();

            foreach (var student in availableStudents)
            {
                var enrollment = await _context.grades_Enrollments
                    .Include(e => e.Group)
                        .ThenInclude(g => g.GradeLevel)
                    .Where(e => e.StudentId == student.UserId)
                    .FirstOrDefaultAsync();

                viewModel.Add(new AvailableStudentViewModel
                {
                    UserId = student.UserId,
                    FullName = $"{student.Person.FirstName} {student.Person.LastNamePaternal} {student.Person.LastNameMaternal}",
                    Email = student.Email,
                    SemesterName = enrollment?.Group?.GradeLevel?.Name,
                    GroupName = enrollment?.Group?.Name
                });
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AsignarAlumno(int studentId)
        {
            int currentTeacherId = GetCurrentTeacherId();

            // Verificar si el alumno ya está asignado a otro maestro
            var assignedToOtherTeacher = await _context.SocialServiceAssignments
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.Person)
                .FirstOrDefaultAsync(a => a.StudentId == studentId 
                    && a.IsActive 
                    && a.TeacherId != currentTeacherId);

            if (assignedToOtherTeacher != null)
            {
                var otherTeacherName = $"{assignedToOtherTeacher.Teacher.Person.FirstName} {assignedToOtherTeacher.Teacher.Person.LastNamePaternal}";
                TempData["Error"] = $"Este alumno ya está asignado al profesor {otherTeacherName}.";
                return RedirectToAction("AgregarAlumnos");
            }

            // Buscar si existe asignación previa (activa o inactiva) con el maestro actual
            var existingAssignment = await _context.SocialServiceAssignments
                .FirstOrDefaultAsync(a => a.TeacherId == currentTeacherId
                    && a.StudentId == studentId);

            if (existingAssignment != null)
            {
                if (!existingAssignment.IsActive)
                {
                    existingAssignment.IsActive = true;
                    existingAssignment.AssignedDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Alumno reasignado exitosamente.";
                }
                else
                {
                    TempData["Error"] = "Este alumno ya está asignado a ti.";
                }
            }
            else
            {
                var assignment = new social_service_assignment
                {
                    TeacherId = currentTeacherId,
                    StudentId = studentId,
                    AssignedDate = DateTime.Now,
                    IsActive = true
                };

                _context.SocialServiceAssignments.Add(assignment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Alumno asignado exitosamente.";
            }

            return RedirectToAction("AgregarAlumnos");
        }

        [HttpPost]
        public async Task<IActionResult> DesasignarAlumno(int studentId)
        {
            int currentTeacherId = GetCurrentTeacherId();

            var assignment = await _context.SocialServiceAssignments
                .FirstOrDefaultAsync(a => a.TeacherId == currentTeacherId
                    && a.StudentId == studentId
                    && a.IsActive);

            if (assignment != null)
            {
                assignment.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Alumno desasignado exitosamente.";
            }

            return RedirectToAction("Alumnos");
        }

        public async Task<IActionResult> RevisarBitacorasAlumno(int id)
        {
            int currentTeacherId = GetCurrentTeacherId();

            // Verificar que el alumno esté asignado a este maestro (validación de seguridad)
            var isAssigned = await _context.SocialServiceAssignments
                .AnyAsync(a => a.TeacherId == currentTeacherId
                    && a.StudentId == id
                    && a.IsActive);

            if (!isAssigned)
            {
                TempData["Error"] = "Este alumno no está asignado a ti.";
                return RedirectToAction("Alumnos");
            }

            var bitacoras = await _context.SocialServiceLogs
                .Include(b => b.Student)
                    .ThenInclude(s => s.Person)
                .Where(b => b.StudentId == id)
                .OrderByDescending(b => b.Week)
                .ThenByDescending(b => b.CreatedAt)
                .ToListAsync();

            var student = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (student != null)
            {
                ViewBag.StudentName = $"{student.Person.FirstName} {student.Person.LastNamePaternal} {student.Person.LastNameMaternal}";
                ViewBag.StudentId = id;
            }

            return View(bitacoras);
        }

        [HttpPost]
        public async Task<IActionResult> AprobarBitacora(int logId, int approvedHoursPracticas, int approvedHoursServicioSocial, string teacherComments)
        {
            int currentTeacherId = GetCurrentTeacherId();

            var bitacora = await _context.SocialServiceLogs
                .FirstOrDefaultAsync(b => b.LogId == logId);

            if (bitacora == null)
            {
                TempData["Error"] = "Bitácora no encontrada.";
                return RedirectToAction("Alumnos");
            }

            // Verificar que el alumno esté asignado a este maestro (validación de seguridad)
            var isAssigned = await _context.SocialServiceAssignments
                .AnyAsync(a => a.TeacherId == currentTeacherId
                    && a.StudentId == bitacora.StudentId
                    && a.IsActive);

            if (!isAssigned)
            {
                TempData["Error"] = "No tienes permiso para aprobar esta bitácora.";
                return RedirectToAction("Alumnos");
            }

            if (approvedHoursPracticas > bitacora.HoursPracticas || approvedHoursServicioSocial > bitacora.HoursServicioSocial)
            {
                TempData["Error"] = "Las horas aprobadas no pueden exceder las horas registradas.";
                return RedirectToAction("RevisarBitacorasAlumno", new { id = bitacora.StudentId });
            }

            bitacora.IsApproved = true;
            bitacora.ApprovedHoursPracticas = approvedHoursPracticas;
            bitacora.ApprovedHoursServicioSocial = approvedHoursServicioSocial;
            bitacora.ApprovedBy = currentTeacherId;
            bitacora.ApprovedAt = DateTime.Now;
            bitacora.TeacherComments = teacherComments;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Bitácora aprobada exitosamente. Las horas se han sumado al alumno.";
            return RedirectToAction("RevisarBitacorasAlumno", new { id = bitacora.StudentId });
        }

        public async Task<IActionResult> AsignarHoras()
        {
            int currentTeacherId = GetCurrentTeacherId();

            var assignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .OrderBy(a => a.Student.Person.FirstName)
                .ToListAsync();

            var viewModel = new List<AsignarHorasViewModel>();

            foreach (var assignment in assignments)
            {
                var bitacorasPendientes = await _context.SocialServiceLogs
                    .Where(log => log.StudentId == assignment.StudentId && !log.IsApproved)
                    .OrderBy(log => log.Week)
                    .Select(log => new BitacoraPendiente
                    {
                        LogId = log.LogId,
                        Week = log.Week,
                        HoursPracticas = log.HoursPracticas,
                        HoursServicioSocial = log.HoursServicioSocial,
                        CreatedAt = log.CreatedAt
                    })
                    .ToListAsync();

                // Solo agregar alumnos que tengan bitácoras pendientes
                if (bitacorasPendientes.Any())
                {
                    var fullName = $"{assignment.Student.Person.FirstName} {assignment.Student.Person.LastNamePaternal} {assignment.Student.Person.LastNameMaternal}";

                    viewModel.Add(new AsignarHorasViewModel
                    {
                        StudentId = assignment.StudentId,
                        StudentName = fullName,
                        GroupName = assignment.GroupName,
                        BitacorasPendientes = bitacorasPendientes,
                        TotalHorasPracticasPendientes = bitacorasPendientes.Sum(b => b.HoursPracticas),
                        TotalHorasServicioSocialPendientes = bitacorasPendientes.Sum(b => b.HoursServicioSocial)
                    });
                }
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Asistencia()
        {
            int currentTeacherId = GetCurrentTeacherId();
            DateTime today = DateTime.Today;

            var assignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .OrderBy(a => a.Student.Person.FirstName)
                .ToListAsync();

            var viewModel = new AttendanceListViewModel
            {
                Today = today
            };

            foreach (var assignment in assignments)
            {
                var todayAttendance = await _context.SocialServiceAttendances
                    .FirstOrDefaultAsync(att => att.StudentId == assignment.StudentId
                        && att.Date.Date == today);

                // Obtener las últimas 5 asistencias incluyendo hoy (para mostrar historial)
                var recentAttendances = await _context.SocialServiceAttendances
                    .Where(att => att.StudentId == assignment.StudentId)
                    .OrderByDescending(att => att.Date)
                    .Take(5)
                    .ToListAsync();

                viewModel.Students.Add(new AttendanceViewModel
                {
                    Assignment = assignment,
                    HasAttendanceToday = todayAttendance != null,
                    IsPresentToday = todayAttendance?.IsPresent ?? false,
                    RecentAttendances = recentAttendances
                });
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarAsistencia(List<int> presentStudents)
        {
            int currentTeacherId = GetCurrentTeacherId();
            DateTime today = DateTime.Today;

            var assignments = await _context.SocialServiceAssignments
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                // Verificar si ya existe asistencia para hoy (actualizar o crear)
                var existingAttendance = await _context.SocialServiceAttendances
                    .FirstOrDefaultAsync(att => att.StudentId == assignment.StudentId
                        && att.Date.Date == today);

                bool isPresent = presentStudents != null && presentStudents.Contains(assignment.StudentId);

                if (existingAttendance != null)
                {
                    existingAttendance.IsPresent = isPresent;
                }
                else
                {
                    var attendance = new social_service_attendance
                    {
                        StudentId = assignment.StudentId,
                        Date = today,
                        IsPresent = isPresent,
                        Tipo = "Servicio Social"
                    };
                    _context.SocialServiceAttendances.Add(attendance);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Asistencia guardada exitosamente.";
            return RedirectToAction("Asistencia");
        }

        private int GetCurrentTeacherId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return 0;
            }

            return int.Parse(userIdClaim);
        }
    }
}
