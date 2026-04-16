using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.Areas.SocialService.ViewModels;
using ClosedXML.Excel;
using DinkToPdf;
using DinkToPdf.Contracts;
using SchoolManager.Helpers;

namespace SchoolManager.Areas.SocialService.Controllers
{
    [Area("SocialService")]
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConverter _converter;

        public TeacherController(AppDbContext context, IConverter converter)
        {
            _context = context;
            _converter = converter;
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

        public async Task<IActionResult> Alumnos(string searchName, string sortBy = "name", string sortOrder = "asc", string semesterFilter = "", string groupFilter = "", string careerFilter = "", int page = 1, int pageSize = 10)
        {
            int currentTeacherId = GetCurrentTeacherId();
            int[] allowedPageSizes = { 10, 25, 50, 100 };
            if (!allowedPageSizes.Contains(pageSize)) pageSize = 10;

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
                .Include(a => a.Student)
                    .ThenInclude(s => s.Preenrollments)
                        .ThenInclude(p => p.Career)
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

                assignment.CareerName = assignment.Student?.Preenrollments?
                    .OrderByDescending(p => p.CreateStat)
                    .Select(p => p.Career != null ? p.Career.name_career : null)
                    .FirstOrDefault();
            }

            // Filtrar por nombre
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string searchNormalized = RemoveAccents(searchName.ToLower());
                assignedStudents = assignedStudents
                    .Where(a =>
                    {
                        string fullName = $"{a.Student.Person.LastNamePaternal} {a.Student.Person.LastNameMaternal} {a.Student.Person.FirstName}";
                        string fullNameNormalized = RemoveAccents(fullName.ToLower());
                        return fullNameNormalized.Contains(searchNormalized);
                    })
                    .ToList();
                ViewBag.SearchName = searchName;
            }

            // Filtrar por semestre
            if (!string.IsNullOrWhiteSpace(semesterFilter))
            {
                assignedStudents = assignedStudents.Where(a => a.SemesterName == semesterFilter).ToList();
            }

            // Filtrar por grupo
            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                assignedStudents = assignedStudents.Where(a => a.GroupName == groupFilter).ToList();
            }

            // Filtrar por carrera
            if (!string.IsNullOrWhiteSpace(careerFilter))
            {
                assignedStudents = assignedStudents.Where(a => a.CareerName == careerFilter).ToList();
            }

            // Obtener listas únicas para los filtros
            var allAssignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Include(a => a.Student)
                    .ThenInclude(s => s.Preenrollments)
                        .ThenInclude(p => p.Career)
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .ToListAsync();

            var allStudentIds = allAssignments.Select(a => a.StudentId).ToList();
            var allEnrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => allStudentIds.Contains(e.StudentId))
                .ToListAsync();

            foreach (var assignment in allAssignments)
            {
                var enrollment = allEnrollments.FirstOrDefault(e => e.StudentId == assignment.StudentId);
                if (enrollment != null)
                {
                    assignment.SemesterName = enrollment.Group?.GradeLevel?.Name;
                    assignment.GroupName = enrollment.Group?.Name;
                }

                assignment.CareerName = assignment.Student?.Preenrollments?
                    .OrderByDescending(p => p.CreateStat)
                    .Select(p => p.Career != null ? p.Career.name_career : null)
                    .FirstOrDefault();
            }

            var semesters = allAssignments
                .Where(a => !string.IsNullOrEmpty(a.SemesterName))
                .Select(a => a.SemesterName)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            var groups = allAssignments
                .Where(a => !string.IsNullOrEmpty(a.GroupName))
                .Select(a => a.GroupName)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            var careers = allAssignments
                .Where(a => !string.IsNullOrEmpty(a.CareerName))
                .Select(a => a.CareerName)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // Aplicar ordenamiento
            assignedStudents = sortBy.ToLower() switch
            {
                "name" => sortOrder == "asc"
                    ? assignedStudents.OrderBy(a => a.Student.Person.LastNamePaternal)
                        .ThenBy(a => a.Student.Person.LastNameMaternal)
                        .ThenBy(a => a.Student.Person.FirstName).ToList()
                    : assignedStudents.OrderByDescending(a => a.Student.Person.LastNamePaternal)
                        .ThenByDescending(a => a.Student.Person.LastNameMaternal)
                        .ThenByDescending(a => a.Student.Person.FirstName).ToList(),

                "semester" => sortOrder == "asc"
                    ? assignedStudents.OrderBy(a => a.SemesterName ?? "").ToList()
                    : assignedStudents.OrderByDescending(a => a.SemesterName ?? "").ToList(),

                "group" => sortOrder == "asc"
                    ? assignedStudents.OrderBy(a => a.GroupName ?? "").ToList()
                    : assignedStudents.OrderByDescending(a => a.GroupName ?? "").ToList(),

                "career" => sortOrder == "asc"
                    ? assignedStudents.OrderBy(a => a.CareerName ?? "").ToList()
                    : assignedStudents.OrderByDescending(a => a.CareerName ?? "").ToList(),

                "date" => sortOrder == "asc"
                    ? assignedStudents.OrderBy(a => a.AssignedDate).ToList()
                    : assignedStudents.OrderByDescending(a => a.AssignedDate).ToList(),

                _ => assignedStudents.OrderBy(a => a.Student.Person.LastNamePaternal).ToList()
            };

            // Calcular paginación
            int totalRecords = assignedStudents.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedStudents = assignedStudents
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ViewBag para paginación y filtros
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentOrder = sortOrder;
            ViewBag.NextOrder = sortOrder == "asc" ? "desc" : "asc";
            ViewBag.Semesters = semesters;
            ViewBag.Groups = groups;
            ViewBag.Careers = careers;
            ViewBag.SemesterFilter = semesterFilter;
            ViewBag.GroupFilter = groupFilter;
            ViewBag.CareerFilter = careerFilter;
            ViewBag.PageSize = pageSize;

            return View(paginatedStudents);
        }

        public async Task<IActionResult> AgregarAlumnos(string searchName, string sortBy = "name", string sortOrder = "asc", string semesterFilter = "", string groupFilter = "", string careerFilter = "", int page = 1, int pageSize = 10)
        {
            int currentTeacherId = GetCurrentTeacherId();
            int[] allowedPageSizes = { 10, 25, 50, 100 };
            if (!allowedPageSizes.Contains(pageSize)) pageSize = 10;

            var assignedStudentIds = await _context.SocialServiceAssignments
                .Where(a => a.IsActive)
                .Select(a => a.StudentId)
                .ToListAsync();

            var availableStudentsQuery = _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Preenrollments)
                    .ThenInclude(p => p.Career)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student" && ur.IsActive)
                    && !assignedStudentIds.Contains(u.UserId)
                    && u.IsActive);

            var availableStudents = await availableStudentsQuery
                .OrderBy(u => u.Person.LastNamePaternal)
                    .ThenBy(u => u.Person.LastNameMaternal)
                    .ThenBy(u => u.Person.FirstName)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string searchNormalized = RemoveAccents(searchName.ToLower());
                availableStudents = availableStudents
                    .Where(u => 
                    {
                        string fullName = $"{u.Person.LastNamePaternal} {u.Person.LastNameMaternal} {u.Person.FirstName}";
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
                var careerName = student.Preenrollments
                    .OrderByDescending(p => p.CreateStat)
                    .Select(p => p.Career != null ? p.Career.name_career : null)
                    .FirstOrDefault();

                viewModel.Add(new AvailableStudentViewModel
                {
                    UserId = student.UserId,
                    FullName = $"{student.Person.LastNamePaternal} {student.Person.LastNameMaternal} {student.Person.FirstName}",
                    Email = student.Email,
                    CareerName = careerName,
                    SemesterName = enrollment?.Group?.GradeLevel?.Name,
                    GroupName = enrollment?.Group?.Name
                });
            }

            // Filtrar por semestre
            if (!string.IsNullOrWhiteSpace(semesterFilter))
            {
                viewModel = viewModel.Where(s => s.SemesterName == semesterFilter).ToList();
            }

            // Filtrar por grupo
            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                viewModel = viewModel.Where(s => s.GroupName == groupFilter).ToList();
            }

            // Filtrar por carrera
            if (!string.IsNullOrWhiteSpace(careerFilter))
            {
                viewModel = viewModel.Where(s => s.CareerName == careerFilter).ToList();
            }

            // Obtener lista de semestres únicos para el filtro
            var semesters = viewModel
                .Where(s => !string.IsNullOrEmpty(s.SemesterName))
                .Select(s => s.SemesterName)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            // Obtener lista de grupos únicos para el filtro
            var groups = viewModel
                .Where(s => !string.IsNullOrEmpty(s.GroupName))
                .Select(s => s.GroupName)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            var careers = viewModel
                .Where(s => !string.IsNullOrEmpty(s.CareerName))
                .Select(s => s.CareerName)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // Aplicar ordenamiento
            viewModel = sortBy.ToLower() switch
            {
                "name" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.FullName).ToList()
                    : viewModel.OrderByDescending(s => s.FullName).ToList(),

                "semester" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.SemesterName ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.SemesterName ?? "").ToList(),

                "career" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.CareerName ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.CareerName ?? "").ToList(),

                "group" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.GroupName ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.GroupName ?? "").ToList(),

                _ => viewModel.OrderBy(s => s.FullName).ToList()
            };

            // Calcular paginación
            int totalRecords = viewModel.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedStudents = viewModel
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ViewBag para paginación y filtros
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentOrder = sortOrder;
            ViewBag.NextOrder = sortOrder == "asc" ? "desc" : "asc";
            ViewBag.Semesters = semesters;
            ViewBag.Groups = groups;
            ViewBag.Careers = careers;
            ViewBag.SemesterFilter = semesterFilter;
            ViewBag.GroupFilter = groupFilter;
            ViewBag.CareerFilter = careerFilter;
            ViewBag.PageSize = pageSize;

            return View(paginatedStudents);
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
                ViewBag.StudentName = $"{student.Person.LastNamePaternal} {student.Person.LastNameMaternal} {student.Person.FirstName}";
                ViewBag.StudentId = id;
            }

            return View(bitacoras);
        }

        [HttpPost]
        public async Task<IActionResult> AprobarBitacora(int logId, string teacherComments)
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

            if (bitacora.IsApproved)
            {
                TempData["Error"] = "Esta bitácora ya fue aprobada.";
                return RedirectToAction("RevisarBitacorasAlumno", new { id = bitacora.StudentId });
            }

            int approvedHoursPracticas = bitacora.HoursPracticas;
            int approvedHoursServicioSocial = bitacora.HoursServicioSocial;

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

        [HttpPost]
        public async Task<IActionResult> RechazarBitacora(int logId, string rejectionReason)
        {
            int currentTeacherId = GetCurrentTeacherId();

            var bitacora = await _context.SocialServiceLogs
                .FirstOrDefaultAsync(b => b.LogId == logId);

            if (bitacora == null)
            {
                TempData["Error"] = "Bitácora no encontrada.";
                return RedirectToAction("Alumnos");
            }

            var isAssigned = await _context.SocialServiceAssignments
                .AnyAsync(a => a.TeacherId == currentTeacherId
                    && a.StudentId == bitacora.StudentId
                    && a.IsActive);

            if (!isAssigned)
            {
                TempData["Error"] = "No tienes permiso para rechazar esta bitácora.";
                return RedirectToAction("Alumnos");
            }

            if (bitacora.IsApproved)
            {
                TempData["Error"] = "No se puede rechazar una bitácora que ya fue aprobada.";
                return RedirectToAction("RevisarBitacorasAlumno", new { id = bitacora.StudentId });
            }

            int studentId = bitacora.StudentId;

            var rejection = new social_service_rejection
            {
                StudentId = bitacora.StudentId,
                Week = bitacora.Week,
                RejectionReason = rejectionReason,
                RejectedBy = currentTeacherId,
                RejectedAt = DateTime.Now
            };

            _context.SocialServiceRejections.Add(rejection);

            _context.SocialServiceLogs.Remove(bitacora);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bitácora rechazada y eliminada. El alumno deberá crear una nueva.";
            return RedirectToAction("RevisarBitacorasAlumno", new { id = studentId });
        }

        public async Task<IActionResult> AsignarHoras(string searchName = "", string sortBy = "name", string sortOrder = "asc", int page = 1, int pageSize = 10, string semesterFilter = "", string groupFilter = "", string careerFilter = "")
        {
            int currentTeacherId = GetCurrentTeacherId();
            int[] allowedPageSizes = { 10, 25, 50, 100 };
            if (!allowedPageSizes.Contains(pageSize)) pageSize = 10;

            var assignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Include(a => a.Student)
                    .ThenInclude(s => s.Preenrollments)
                        .ThenInclude(p => p.Career)
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .ToListAsync();

            var studentIds = assignments.Select(a => a.StudentId).ToList();

            // Obtener enrollments para semestre y grupo
            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => studentIds.Contains(e.StudentId))
                .ToListAsync();

            // Obtener todas las bitácoras (pendientes y aprobadas)
            var allLogs = await _context.SocialServiceLogs
                .Where(log => studentIds.Contains(log.StudentId))
                .ToListAsync();

            var logsByStudent = allLogs
                .GroupBy(log => log.StudentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var allViewModel = new List<AsignarHorasViewModel>();

            foreach (var assignment in assignments)
            {
                var enrollment = enrollments.FirstOrDefault(e => e.StudentId == assignment.StudentId);
                var semesterName = enrollment?.Group?.GradeLevel?.Name;
                var groupName = enrollment?.Group?.Name ?? assignment.GroupName;

                var studentLogs = logsByStudent.ContainsKey(assignment.StudentId)
                    ? logsByStudent[assignment.StudentId]
                    : new List<social_service_log>();

                var pendingLogs = studentLogs.Where(l => !l.IsApproved).ToList();
                var approvedLogs = studentLogs.Where(l => l.IsApproved).ToList();
                var careerName = assignment.Student?.Preenrollments?
                    .OrderByDescending(p => p.CreateStat)
                    .Select(p => p.Career != null ? p.Career.name_career : null)
                    .FirstOrDefault();

                var fullName = $"{assignment.Student.Person.LastNamePaternal} {assignment.Student.Person.LastNameMaternal} {assignment.Student.Person.FirstName}";

                allViewModel.Add(new AsignarHorasViewModel
                {
                    StudentId = assignment.StudentId,
                    StudentName = fullName,
                    CareerName = careerName,
                    SemesterName = semesterName,
                    GroupName = groupName,
                    TotalBitacoras = studentLogs.Count,
                    BitacorasPendientesCount = pendingLogs.Count,
                    TotalHorasPracticas = approvedLogs.Sum(l => l.ApprovedHoursPracticas),
                    TotalHorasServicioSocial = approvedLogs.Sum(l => l.ApprovedHoursServicioSocial),
                    BitacorasPendientes = pendingLogs.Select(l => new BitacoraPendiente
                    {
                        LogId = l.LogId,
                        Week = l.Week,
                        HoursPracticas = l.HoursPracticas,
                        HoursServicioSocial = l.HoursServicioSocial,
                        CreatedAt = l.CreatedAt
                    }).ToList(),
                    TotalHorasPracticasPendientes = pendingLogs.Sum(l => l.HoursPracticas),
                    TotalHorasServicioSocialPendientes = pendingLogs.Sum(l => l.HoursServicioSocial)
                });
            }

            // Obtener todos los semestres y grupos posibles ANTES de filtrar
            var allSemesters = allViewModel.Select(x => x.SemesterName).Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x).ToList();
            var allGroups = allViewModel.Select(x => x.GroupName).Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x).ToList();
            var allCareers = allViewModel.Select(x => x.CareerName).Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x).ToList();

            // Filtrar por nombre
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string searchNormalized = RemoveAccents(searchName.ToLower());
                allViewModel = allViewModel
                    .Where(v => RemoveAccents(v.StudentName.ToLower()).Contains(searchNormalized))
                    .ToList();
            }

            // Filtrar por semestre
            if (!string.IsNullOrWhiteSpace(semesterFilter))
            {
                allViewModel = allViewModel.Where(v => v.SemesterName == semesterFilter).ToList();
            }

            // Filtrar por grupo
            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                allViewModel = allViewModel.Where(v => v.GroupName == groupFilter).ToList();
            }

            // Filtrar por carrera
            if (!string.IsNullOrWhiteSpace(careerFilter))
            {
                allViewModel = allViewModel.Where(v => v.CareerName == careerFilter).ToList();
            }

            // Aplicar ordenamiento
            allViewModel = sortBy.ToLower() switch
            {
                "name" => sortOrder == "asc"
                    ? allViewModel.OrderBy(a => a.StudentName).ToList()
                    : allViewModel.OrderByDescending(a => a.StudentName).ToList(),
                "career" => sortOrder == "asc"
                    ? allViewModel.OrderBy(a => a.CareerName ?? "").ToList()
                    : allViewModel.OrderByDescending(a => a.CareerName ?? "").ToList(),
                "semester" => sortOrder == "asc"
                    ? allViewModel.OrderBy(a => a.SemesterName ?? "").ToList()
                    : allViewModel.OrderByDescending(a => a.SemesterName ?? "").ToList(),
                "group" => sortOrder == "asc"
                    ? allViewModel.OrderBy(a => a.GroupName ?? "").ToList()
                    : allViewModel.OrderByDescending(a => a.GroupName ?? "").ToList(),
                "totalbitacoras" => sortOrder == "asc"
                    ? allViewModel.OrderBy(a => a.TotalBitacoras).ToList()
                    : allViewModel.OrderByDescending(a => a.TotalBitacoras).ToList(),
                // 'pendientes' sorting removed as column is no longer displayed
                "practicas" => sortOrder == "asc"
                    ? allViewModel.OrderBy(a => a.TotalHorasPracticas).ToList()
                    : allViewModel.OrderByDescending(a => a.TotalHorasPracticas).ToList(),
                "servicio" => sortOrder == "asc"
                    ? allViewModel.OrderBy(a => a.TotalHorasServicioSocial).ToList()
                    : allViewModel.OrderByDescending(a => a.TotalHorasServicioSocial).ToList(),
                "validacion" => sortOrder == "asc"
                    ? allViewModel.OrderBy(a => a.TotalHorasPracticas + a.TotalHorasServicioSocial).ToList()
                    : allViewModel.OrderByDescending(a => a.TotalHorasPracticas + a.TotalHorasServicioSocial).ToList(),
                _ => allViewModel.OrderBy(a => a.StudentName).ToList()
            };

            // Totales globales (antes de paginar)
            ViewBag.TotalBitacorasPendientes = allViewModel.Sum(a => a.BitacorasPendientesCount);
            ViewBag.TotalHorasPracticas = allViewModel.Sum(a => a.TotalHorasPracticas);
            ViewBag.TotalHorasServicioSocial = allViewModel.Sum(a => a.TotalHorasServicioSocial);
            ViewBag.TotalBitacoras = allViewModel.Sum(a => a.TotalBitacoras);

            // Paginación
            int totalRecords = allViewModel.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedViewModel = allViewModel
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Para los selects de filtro (usar todos los posibles, no solo los filtrados)
            ViewBag.Careers = allCareers;
            ViewBag.Semesters = allSemesters;
            ViewBag.Groups = allGroups;
            ViewBag.CareerFilter = careerFilter;
            ViewBag.SemesterFilter = semesterFilter;
            ViewBag.GroupFilter = groupFilter;
            ViewBag.SearchName = searchName;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentOrder = sortOrder;

            return View(paginatedViewModel);
        }

        public async Task<IActionResult> Asistencia(string sortBy = "name", string sortOrder = "asc", string groupFilter = "", string searchName = "", int page = 1, int pageSize = 10, string dateFrom = "", string dateTo = "")
        {
            int currentTeacherId = GetCurrentTeacherId();
            DateTime today = DateTime.Today;
            int[] allowedPageSizes = { 10, 25, 50, 100 };
            if (!allowedPageSizes.Contains(pageSize)) pageSize = 10;

            DateTime? filterDateFrom = null;
            DateTime? filterDateTo = null;
            if (DateTime.TryParse(dateFrom, out var parsedFrom)) filterDateFrom = parsedFrom.Date;
            if (DateTime.TryParse(dateTo, out var parsedTo)) filterDateTo = parsedTo.Date;

            var assignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
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

            // Filtrar por grupo
            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                assignments = assignments.Where(a => a.GroupName == groupFilter).ToList();
            }

            // Filtrar por nombre
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string searchNormalized = RemoveAccents(searchName.ToLower());
                assignments = assignments
                    .Where(a =>
                    {
                        string fullName = $"{a.Student.Person.LastNamePaternal} {a.Student.Person.LastNameMaternal} {a.Student.Person.FirstName}";
                        return RemoveAccents(fullName.ToLower()).Contains(searchNormalized);
                    })
                    .ToList();
            }

            // Obtener lista de grupos únicos para el filtro
            var allAssignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .ToListAsync();

            var allStudentIds = allAssignments.Select(a => a.StudentId).ToList();
            var allEnrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => allStudentIds.Contains(e.StudentId))
                .ToListAsync();

            foreach (var assignment in allAssignments)
            {
                var enrollment = allEnrollments.FirstOrDefault(e => e.StudentId == assignment.StudentId);
                if (enrollment != null)
                {
                    assignment.GroupName = enrollment.Group?.Name;
                }
            }

            var groups = allAssignments
                .Where(a => !string.IsNullOrEmpty(a.GroupName))
                .Select(a => a.GroupName)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            // Aplicar ordenamiento
            assignments = sortBy.ToLower() switch
            {
                "group" => sortOrder == "asc"
                    ? assignments.OrderBy(a => a.GroupName ?? "").ToList()
                    : assignments.OrderByDescending(a => a.GroupName ?? "").ToList(),

                _ => sortOrder == "asc"
                    ? assignments.OrderBy(a => a.Student.Person.LastNamePaternal)
                        .ThenBy(a => a.Student.Person.LastNameMaternal)
                        .ThenBy(a => a.Student.Person.FirstName)
                        .ToList()
                    : assignments.OrderByDescending(a => a.Student.Person.LastNamePaternal)
                        .ThenByDescending(a => a.Student.Person.LastNameMaternal)
                        .ThenByDescending(a => a.Student.Person.FirstName)
                        .ToList()
            };

            // Paginación
            int totalRecords = assignments.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedAssignments = assignments
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paginatedStudentIds = paginatedAssignments.Select(a => a.StudentId).ToList();

            var todayAttendances = await _context.SocialServiceAttendances
                .Where(att => studentIds.Contains(att.StudentId) && att.Date.Date == today)
                .ToListAsync();

            var allAttendancesQuery = _context.SocialServiceAttendances
                .Where(att => paginatedStudentIds.Contains(att.StudentId));

            if (filterDateFrom.HasValue)
                allAttendancesQuery = allAttendancesQuery.Where(att => att.Date.Date >= filterDateFrom.Value);
            if (filterDateTo.HasValue)
                allAttendancesQuery = allAttendancesQuery.Where(att => att.Date.Date <= filterDateTo.Value);

            var allAttendances = await allAttendancesQuery
                .OrderByDescending(att => att.Date)
                .ToListAsync();

            var attendancesByStudent = allAttendances
                .GroupBy(att => att.StudentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var viewModel = new AttendanceListViewModel
            {
                Today = today
            };

            foreach (var assignment in paginatedAssignments)
            {
                var todayAttendance = todayAttendances.FirstOrDefault(att => att.StudentId == assignment.StudentId);
                attendancesByStudent.TryGetValue(assignment.StudentId, out var recentAttendances);

                viewModel.Students.Add(new AttendanceViewModel
                {
                    Assignment = assignment,
                    HasAttendanceToday = todayAttendance != null,
                    IsPresentToday = todayAttendance?.IsPresent ?? false,
                    RecentAttendances = recentAttendances ?? new List<social_service_attendance>()
                });
            }

            // ViewBag para filtros y paginación
            ViewBag.CurrentSort = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.NextOrder = sortOrder == "asc" ? "desc" : "asc";
            ViewBag.Groups = groups;
            ViewBag.GroupFilter = groupFilter;
            ViewBag.SearchName = searchName;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarAsistencia(List<int> presentStudents, List<int> allStudentsOnPage, string sortOrder = "asc", string groupFilter = "", string searchName = "", int page = 1, int pageSize = 10, string dateFrom = "", string dateTo = "")
        {
            int currentTeacherId = GetCurrentTeacherId();
            DateTime today = DateTime.Today;

            var assignments = await _context.SocialServiceAssignments
                .Where(a => a.TeacherId == currentTeacherId && a.IsActive)
                .ToListAsync();

            var studentsToProcess = allStudentsOnPage != null && allStudentsOnPage.Any()
                ? assignments.Where(a => allStudentsOnPage.Contains(a.StudentId)).ToList()
                : assignments;

            foreach (var assignment in studentsToProcess)
            {
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
            return RedirectToAction("Asistencia", new { sortOrder, groupFilter, searchName, page, pageSize, dateFrom, dateTo });
        }

        private async Task<List<AsignarHorasViewModel>> GetAvanceData(int teacherId, string searchName = "", string semesterFilter = "", string groupFilter = "", string careerFilter = "")
        {
            var assignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Include(a => a.Student)
                    .ThenInclude(s => s.Preenrollments)
                        .ThenInclude(p => p.Career)
                .Where(a => a.TeacherId == teacherId && a.IsActive)
                .OrderBy(a => a.Student.Person.LastNamePaternal)
                    .ThenBy(a => a.Student.Person.LastNameMaternal)
                    .ThenBy(a => a.Student.Person.FirstName)
                .ToListAsync();

            var studentIds = assignments.Select(a => a.StudentId).ToList();

            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => studentIds.Contains(e.StudentId))
                .ToListAsync();

            var allLogs = await _context.SocialServiceLogs
                .Where(log => studentIds.Contains(log.StudentId))
                .ToListAsync();

            var logsByStudent = allLogs
                .GroupBy(log => log.StudentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<AsignarHorasViewModel>();

            foreach (var assignment in assignments)
            {
                var enrollment = enrollments.FirstOrDefault(e => e.StudentId == assignment.StudentId);
                var studentLogs = logsByStudent.ContainsKey(assignment.StudentId)
                    ? logsByStudent[assignment.StudentId]
                    : new List<social_service_log>();

                var pendingLogs = studentLogs.Where(l => !l.IsApproved).ToList();
                var approvedLogs = studentLogs.Where(l => l.IsApproved).ToList();
                var careerName = assignment.Student?.Preenrollments?
                    .OrderByDescending(p => p.CreateStat)
                    .Select(p => p.Career != null ? p.Career.name_career : null)
                    .FirstOrDefault();

                result.Add(new AsignarHorasViewModel
                {
                    StudentId = assignment.StudentId,
                    StudentName = $"{assignment.Student.Person.LastNamePaternal} {assignment.Student.Person.LastNameMaternal} {assignment.Student.Person.FirstName}",
                    CareerName = careerName,
                    SemesterName = enrollment?.Group?.GradeLevel?.Name,
                    GroupName = enrollment?.Group?.Name ?? assignment.GroupName,
                    TotalBitacoras = studentLogs.Count,
                    BitacorasPendientesCount = pendingLogs.Count,
                    TotalHorasPracticas = approvedLogs.Sum(l => l.ApprovedHoursPracticas),
                    TotalHorasServicioSocial = approvedLogs.Sum(l => l.ApprovedHoursServicioSocial)
                });
            }

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string searchNormalized = RemoveAccents(searchName.ToLower());
                result = result
                    .Where(v => RemoveAccents(v.StudentName.ToLower()).Contains(searchNormalized))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(semesterFilter))
            {
                result = result.Where(v => v.SemesterName == semesterFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                result = result.Where(v => v.GroupName == groupFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(careerFilter))
            {
                result = result.Where(v => v.CareerName == careerFilter).ToList();
            }

            return result;
        }

        public async Task<IActionResult> ExportAvanceExcel(string searchName = "", string semesterFilter = "", string groupFilter = "", string careerFilter = "")
        {
            int currentTeacherId = GetCurrentTeacherId();
            if (currentTeacherId == 0) return RedirectToAction("Login", "Account", new { area = "UserMng" });

            var data = await GetAvanceData(currentTeacherId, searchName, semesterFilter, groupFilter, careerFilter);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Avance del Alumno");

            // Encabezados
            var headers = new[] { "Alumno", "Carrera", "Semestre", "Grupo", "Total Bitácoras", "Horas Prácticas", "Horas Servicio Social", "Validación" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#8C1B1B");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int row = 2;
            foreach (var alumno in data)
            {
                int totalHoras = alumno.TotalHorasPracticas + alumno.TotalHorasServicioSocial;
                double porcentaje = (totalHoras / 720.0) * 100;

                string validacion = porcentaje >= 100 ? "100%" : porcentaje >= 75 ? "75%" : porcentaje >= 50 ? "50%" : porcentaje >= 25 ? "25%" : "0%";

                ws.Cell(row, 1).Value = alumno.StudentName;
                ws.Cell(row, 2).Value = alumno.CareerName ?? "N/A";
                ws.Cell(row, 3).Value = alumno.SemesterName ?? "N/A";
                ws.Cell(row, 4).Value = alumno.GroupName ?? "N/A";
                ws.Cell(row, 5).Value = alumno.TotalBitacoras;
                ws.Cell(row, 6).Value = $"{alumno.TotalHorasPracticas} h";
                ws.Cell(row, 7).Value = $"{alumno.TotalHorasServicioSocial} h";
                ws.Cell(row, 8).Value = validacion;

                for (int c = 2; c <= 8; c++)
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Avance_Alumnos_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportAvancePdf(string searchName = "", string semesterFilter = "", string groupFilter = "", string careerFilter = "")
        {
            int currentTeacherId = GetCurrentTeacherId();
            if (currentTeacherId == 0) return RedirectToAction("Login", "Account", new { area = "UserMng" });

            var data = await GetAvanceData(currentTeacherId, searchName, semesterFilter, groupFilter, careerFilter);

            string htmlContent = await this.RenderViewAsync("_AvancePdf", data, true);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.Letter,
                Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" },
                HeaderSettings = { Line = false },
                FooterSettings = { Center = "Página [page] de [toPage]", FontSize = 8, Line = false }
            };

            var pdf = new HtmlToPdfDocument
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);
            return File(file, "application/pdf", $"Avance_Alumnos_{DateTime.Now:yyyyMMdd}.pdf");
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

            if (int.TryParse(weekString.Trim(), out int weekNumber))
            {
                return weekNumber;
            }

            var parts = weekString.Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[1], out weekNumber))
            {
                return weekNumber;
            }

            return 0;
        }
    }
}
