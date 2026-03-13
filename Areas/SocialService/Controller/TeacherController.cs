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

            var studentIds = assignedStudents.Select(a => a.StudentId).ToList();

            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => studentIds.Contains(e.StudentId))
                .ToListAsync();

            foreach (var assignment in assignedStudents)
            {
                var enrollment = enrollments.FirstOrDefault(e => e.StudentId == assignment.StudentId);

                if (enrollment != null)
                {
                    assignment.SemesterName = enrollment.Group?.GradeLevel?.Name;
                    assignment.GroupName = enrollment.Group?.Name;
                }
            }

            return View(assignedStudents);
        }

        public async Task<IActionResult> AgregarAlumnos(string searchName)
        {
            int currentTeacherId = GetCurrentTeacherId();

            var assignedStudentIds = await _context.SocialServiceAssignments
                .Where(a => a.IsActive)
                .Select(a => a.StudentId)
                .ToListAsync();

            var availableStudentsQuery = _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student" && ur.IsActive)
                    && !assignedStudentIds.Contains(u.UserId)
                    && u.IsActive);

            var availableStudents = await availableStudentsQuery
                .OrderBy(u => u.Person.FirstName)
                    .ThenBy(u => u.Person.LastNamePaternal)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string searchNormalized = RemoveAccents(searchName.ToLower());
                availableStudents = availableStudents
                    .Where(u => 
                    {
                        string fullName = $"{u.Person.FirstName} {u.Person.LastNamePaternal} {u.Person.LastNameMaternal}";
                        string fullNameNormalized = RemoveAccents(fullName.ToLower());
                        return fullNameNormalized.Contains(searchNormalized);
                    })
                    .ToList();
                ViewBag.SearchName = searchName;
            }

            var viewModel = new List<AvailableStudentViewModel>();

            var studentIds = availableStudents.Select(s => s.UserId).ToList();

            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => studentIds.Contains(e.StudentId))
                .ToListAsync();

            foreach (var student in availableStudents)
            {
                var enrollment = enrollments.FirstOrDefault(e => e.StudentId == student.UserId);

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
                .ToListAsync();

            bitacoras = bitacoras
                .OrderByDescending(b => ExtractWeekNumber(b.Week))
                .ThenByDescending(b => b.CreatedAt)
                .ToList();

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

            // Lógica de límite: calcular horas acumuladas para no exceder 240/480
            var approvedLogs = await _context.SocialServiceLogs
                .Where(log => log.StudentId == bitacora.StudentId && log.IsApproved && log.LogId != logId)
                .ToListAsync();

            int currentTotalPracticas = approvedLogs.Sum(log => log.ApprovedHoursPracticas);
            int currentTotalServicioSocial = approvedLogs.Sum(log => log.ApprovedHoursServicioSocial);

            const int requiredHoursPracticas = 240;
            const int requiredHoursServicioSocial = 480;

            int remainingHoursPracticas = Math.Max(0, requiredHoursPracticas - currentTotalPracticas);
            int remainingHoursServicioSocial = Math.Max(0, requiredHoursServicioSocial - currentTotalServicioSocial);

            int finalApprovedPracticas = Math.Min(approvedHoursPracticas, remainingHoursPracticas);
            int finalApprovedServicioSocial = Math.Min(approvedHoursServicioSocial, remainingHoursServicioSocial);

            List<string> adjustmentMessages = new List<string>();

            if (finalApprovedPracticas < approvedHoursPracticas)
            {
                if (remainingHoursPracticas == 0)
                {
                    adjustmentMessages.Add($"El alumno ya completó las 240 horas de Prácticas Profesionales. No se sumaron las {approvedHoursPracticas} horas de esta categoría.");
                }
                else
                {
                    adjustmentMessages.Add($"Prácticas Profesionales: Solo se aprobaron {finalApprovedPracticas} de {approvedHoursPracticas} horas porque el alumno solo necesitaba {remainingHoursPracticas} horas para completar.");
                }
            }

            if (finalApprovedServicioSocial < approvedHoursServicioSocial)
            {
                if (remainingHoursServicioSocial == 0)
                {
                    adjustmentMessages.Add($"El alumno ya completó las 480 horas de Servicio Social. No se sumaron las {approvedHoursServicioSocial} horas de esta categoría.");
                }
                else
                {
                    adjustmentMessages.Add($"Servicio Social: Solo se aprobaron {finalApprovedServicioSocial} de {approvedHoursServicioSocial} horas porque el alumno solo necesitaba {remainingHoursServicioSocial} horas para completar.");
                }
            }

            bitacora.IsApproved = true;
            bitacora.ApprovedHoursPracticas = finalApprovedPracticas;
            bitacora.ApprovedHoursServicioSocial = finalApprovedServicioSocial;
            bitacora.ApprovedBy = currentTeacherId;
            bitacora.ApprovedAt = DateTime.Now;
            bitacora.TeacherComments = teacherComments;

            await _context.SaveChangesAsync();

            // Mensaje de éxito con información de ajustes si los hubo
            if (adjustmentMessages.Any())
            {
                string adjustmentInfo = string.Join(" ", adjustmentMessages);
                TempData["Success"] = $"Bitácora aprobada. {adjustmentInfo}";
            }
            else
            {
                TempData["Success"] = "Bitácora aprobada exitosamente. Las horas se han sumado al alumno.";
            }

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

            var studentIds = assignments.Select(a => a.StudentId).ToList();

            var bitacorasPendientesQuery = await _context.SocialServiceLogs
                .Where(log => studentIds.Contains(log.StudentId) && !log.IsApproved)
                .OrderBy(log => log.Week)
                .Select(log => new 
                {
                    log.StudentId,
                    BitacoraPendiente = new BitacoraPendiente
                    {
                        LogId = log.LogId,
                        Week = log.Week,
                        HoursPracticas = log.HoursPracticas,
                        HoursServicioSocial = log.HoursServicioSocial,
                        CreatedAt = log.CreatedAt
                    }
                })
                .ToListAsync();

            var bitacorasByStudent = bitacorasPendientesQuery
                .GroupBy(b => b.StudentId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.BitacoraPendiente).ToList());

            foreach (var assignment in assignments)
            {
                if (bitacorasByStudent.TryGetValue(assignment.StudentId, out var bitacorasPendientes) && bitacorasPendientes.Any())
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

            var studentIds = assignments.Select(a => a.StudentId).ToList();

            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => studentIds.Contains(e.StudentId))
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                var enrollment = enrollments.FirstOrDefault(e => e.StudentId == assignment.StudentId);

                if (enrollment != null)
                {
                    assignment.SemesterName = enrollment.Group?.GradeLevel?.Name;
                    assignment.GroupName = enrollment.Group?.Name;
                }
            }

            var todayAttendances = await _context.SocialServiceAttendances
                .Where(att => studentIds.Contains(att.StudentId) && att.Date.Date == today)
                .ToListAsync();

            var recentAttendancesQuery = await _context.SocialServiceAttendances
                .Where(att => studentIds.Contains(att.StudentId))
                .OrderByDescending(att => att.Date)
                .ToListAsync();

            var recentAttendancesByStudent = recentAttendancesQuery
                .GroupBy(att => att.StudentId)
                .ToDictionary(g => g.Key, g => g.Take(5).ToList());

            var viewModel = new AttendanceListViewModel
            {
                Today = today
            };

            foreach (var assignment in assignments)
            {
                var todayAttendance = todayAttendances.FirstOrDefault(att => att.StudentId == assignment.StudentId);
                recentAttendancesByStudent.TryGetValue(assignment.StudentId, out var recentAttendances);

                viewModel.Students.Add(new AttendanceViewModel
                {
                    Assignment = assignment,
                    HasAttendanceToday = todayAttendance != null,
                    IsPresentToday = todayAttendance?.IsPresent ?? false,
                    RecentAttendances = recentAttendances ?? new List<social_service_attendance>()
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

        private string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        private int ExtractWeekNumber(string weekString)
        {
            if (string.IsNullOrWhiteSpace(weekString))
                return 0;

            var parts = weekString.Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int weekNumber))
            {
                return weekNumber;
            }

            return 0;
        }
    }
}
