using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // Al acceder a /SocialService/Teacher redirige al dashboard del profesor
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Dashboard()
        {
            int currentTeacherId = GetCurrentTeacherId();
            DateTime today = DateTime.Today;

            // 1. Obtener cantidad de alumnos asignados
            var alumnosAsignados = await _context.SocialServiceAssignments
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .CountAsync();

            // 2. Obtener cantidad de bitácoras pendientes de aprobar
            var studentIds = await _context.SocialServiceAssignments
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .Select(a => a.StudentId)
                .ToListAsync();

            var bitacorasPendientes = await _context.SocialServiceLogs
                .Where(log => studentIds.Contains(log.StudentId) && !log.IsApproved)
                .CountAsync();

            // 3. Obtener cantidad de asistencias registradas hoy
            var asistenciasHoy = await _context.SocialServiceAttendances
                .Where(att => studentIds.Contains(att.StudentId)
                    && att.Date.Date == today
                    && att.IsPresent)
                .CountAsync();

            // Pasar las estadísticas a la vista
            ViewBag.AlumnosAsignados = alumnosAsignados;
            ViewBag.BitacorasPendientes = bitacorasPendientes;
            ViewBag.AsistenciasHoy = asistenciasHoy;

            return View();
        }

        public async Task<IActionResult> Alumnos()
        {
            // TODO: Obtener el ID del maestro actual desde la sesión/autenticación
            // Por ahora usaremos un valor temporal
            int currentTeacherId = GetCurrentTeacherId();

            var assignedStudents = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Include(a => a.Student.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .ToListAsync();

            return View(assignedStudents);
        }

        // Vista para agregar alumnos
        public async Task<IActionResult> AgregarAlumnos()
        {
            int currentTeacherId = GetCurrentTeacherId();

            // Obtener todos los estudiantes que NO están asignados ACTIVAMENTE a este maestro
            var assignedStudentIds = await _context.SocialServiceAssignments
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .Select(a => a.StudentId)
                .ToListAsync();

            var availableStudents = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student" && ur.IsActive)
                    && !assignedStudentIds.Contains(u.UserId)
                    && u.IsActive)
                .ToListAsync();

            return View(availableStudents);
        }

        // Asignar alumno al maestro
        [HttpPost]
        public async Task<IActionResult> AsignarAlumno(int studentId)
        {
            int currentTeacherId = GetCurrentTeacherId();

            // Buscar si existe alguna asignación (activa o inactiva) para esta combinación
            var existingAssignment = await _context.SocialServiceAssignments
                .FirstOrDefaultAsync(a => a.TeacherId == currentTeacherId
                    && a.StudentId == studentId);

            if (existingAssignment != null)
            {
                // Si existe pero está inactiva, reactivarla
                if (!existingAssignment.IsActive)
                {
                    existingAssignment.IsActive = true;
                    existingAssignment.AssignedDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Alumno reasignado exitosamente.";
                }
                else
                {
                    // Ya está activa
                    TempData["Error"] = "Este alumno ya está asignado.";
                }
            }
            else
            {
                // No existe, crear nueva asignación
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

        // Desasignar alumno
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

        // Ver bitácoras de un alumno específico
        public async Task<IActionResult> RevisarBitacorasAlumno(int id)
        {
            int currentTeacherId = GetCurrentTeacherId();

            // Verificar que el alumno esté asignado a este maestro
            var isAssigned = await _context.SocialServiceAssignments
                .AnyAsync(a => a.TeacherId == currentTeacherId
                    && a.StudentId == id
                    && a.IsActive);

            if (!isAssigned)
            {
                TempData["Error"] = "Este alumno no está asignado a ti.";
                return RedirectToAction("Alumnos");
            }

            // Obtener bitácoras del alumno
            var bitacoras = await _context.SocialServiceLogs
                .Include(b => b.Student)
                    .ThenInclude(s => s.Person)
                .Where(b => b.StudentId == id)
                .OrderByDescending(b => b.Week)
                .ThenByDescending(b => b.CreatedAt)
                .ToListAsync();

            // Obtener información del estudiante
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

        // Aprobar bitácora y sumar horas
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

            // Verificar que el alumno esté asignado a este maestro
            var isAssigned = await _context.SocialServiceAssignments
                .AnyAsync(a => a.TeacherId == currentTeacherId
                    && a.StudentId == bitacora.StudentId
                    && a.IsActive);

            if (!isAssigned)
            {
                TempData["Error"] = "No tienes permiso para aprobar esta bitácora.";
                return RedirectToAction("Alumnos");
            }

            // Validar que las horas aprobadas no excedan las registradas
            if (approvedHoursPracticas > bitacora.HoursPracticas || approvedHoursServicioSocial > bitacora.HoursServicioSocial)
            {
                TempData["Error"] = "Las horas aprobadas no pueden exceder las horas registradas.";
                return RedirectToAction("RevisarBitacorasAlumno", new { id = bitacora.StudentId });
            }

            // Actualizar la bitácora
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

        // Vista de asignar horas - Resumen de bitácoras pendientes
        public async Task<IActionResult> AsignarHoras()
        {
            int currentTeacherId = GetCurrentTeacherId();

            // Obtener alumnos asignados al maestro
            var assignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .OrderBy(a => a.Student.Person.FirstName)
                .ToListAsync();

            var viewModel = new List<AsignarHorasViewModel>();

            foreach (var assignment in assignments)
            {
                // Obtener bitácoras pendientes de aprobar
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

            // Obtener alumnos asignados al maestro
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
                // Verificar si ya tiene asistencia registrada hoy
                var todayAttendance = await _context.SocialServiceAttendances
                    .FirstOrDefaultAsync(att => att.StudentId == assignment.StudentId
                        && att.Date.Date == today);

                // Obtener las últimas 5 asistencias (incluyendo hoy si existe)
                // Esto asegura que siempre veamos el historial completo
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

        // Guardar asistencia del día
        [HttpPost]
        public async Task<IActionResult> GuardarAsistencia(List<int> presentStudents)
        {
            int currentTeacherId = GetCurrentTeacherId();
            DateTime today = DateTime.Today;

            // Obtener todos los alumnos asignados
            var assignments = await _context.SocialServiceAssignments
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                // Verificar si ya existe asistencia para hoy
                var existingAttendance = await _context.SocialServiceAttendances
                    .FirstOrDefaultAsync(att => att.StudentId == assignment.StudentId
                        && att.Date.Date == today);

                bool isPresent = presentStudents != null && presentStudents.Contains(assignment.StudentId);

                if (existingAttendance != null)
                {
                    // Actualizar asistencia existente
                    existingAttendance.IsPresent = isPresent;
                }
                else
                {
                    // Crear nueva asistencia
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

        // Método auxiliar para obtener el ID del maestro actual
        // TODO: Implementar la lógica real de autenticación
        private int GetCurrentTeacherId()
        {
            // Por ahora retorna un valor fijo para testing
            // En producción, esto debe obtener el UserId del usuario autenticado
            // Ejemplo: return int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            return 4; // ID de Angel Gael Villedaaa - Valor temporal para pruebas
        }
    }
}
