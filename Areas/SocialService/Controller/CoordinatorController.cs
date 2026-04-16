using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class CoordinatorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConverter _converter;

        public CoordinatorController(AppDbContext context, IConverter converter)
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
            var teacherIds = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.Name == "Teacher" && ur.IsActive)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var totalTeachers = teacherIds.Count;

            var totalStudents = await _context.SocialServiceAssignments
                .Where(a => a.IsActive)
                .Select(a => a.StudentId)
                .Distinct()
                .CountAsync();

            var bitacorasPendientes = await _context.SocialServiceLogs
                .Where(log => !log.IsApproved)
                .CountAsync();

            var asistenciasHoy = await _context.SocialServiceAttendances
                .Where(att => att.Date.Date == DateTime.Today && att.IsPresent)
                .CountAsync();

            var totalBitacoras = await _context.SocialServiceLogs.CountAsync();

            ViewBag.TotalMaestros = totalTeachers;
            ViewBag.TotalAlumnos = totalStudents;
            ViewBag.BitacorasPendientes = bitacorasPendientes;
            ViewBag.AsistenciasHoy = asistenciasHoy;
            ViewBag.TotalBitacoras = totalBitacoras;

            return View();
        }

        public async Task<IActionResult> Maestros(string searchName = "", string searchEmail = "", string sortBy = "name", string sortOrder = "asc", int page = 1, int pageSize = 10)
        {
            int[] allowedPageSizes = { 10, 25, 50, 100 };
            if (!allowedPageSizes.Contains(pageSize)) pageSize = 10;

            var assignments = await _context.SocialServiceAssignments
                .Where(a => a.IsActive)
                .ToListAsync();

            var assignedTeacherIds = assignments
                .Select(a => a.TeacherId)
                .Distinct()
                .ToList();

            var teacherUserIds = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.Name == "Teacher" && ur.IsActive && assignedTeacherIds.Contains(ur.UserId))
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();

            var teachers = await _context.Users
                .Include(u => u.Person)
                .Where(u => teacherUserIds.Contains(u.UserId) && u.IsActive)
                .ToListAsync();

            var studentIds = assignments.Select(a => a.StudentId).Distinct().ToList();

            var allLogs = await _context.SocialServiceLogs
                .Where(log => studentIds.Contains(log.StudentId))
                .ToListAsync();

            var viewModel = new List<CoordinatorTeacherViewModel>();

            foreach (var teacher in teachers)
            {
                var teacherAssignments = assignments.Where(a => a.TeacherId == teacher.UserId).ToList();
                var teacherStudentIds = teacherAssignments.Select(a => a.StudentId).ToList();
                var teacherLogs = allLogs.Where(l => teacherStudentIds.Contains(l.StudentId)).ToList();

                viewModel.Add(new CoordinatorTeacherViewModel
                {
                    TeacherId = teacher.UserId,
                    TeacherName = $"{teacher.Person.LastNamePaternal} {teacher.Person.LastNameMaternal} {teacher.Person.FirstName}",
                    Email = teacher.Email,
                    AlumnosAsignados = teacherAssignments.Count,
                    TotalBitacoras = teacherLogs.Count,
                    BitacorasPendientes = teacherLogs.Count(l => !l.IsApproved)
                });
            }

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string searchNormalized = RemoveAccents(searchName.ToLower());
                viewModel = viewModel
                    .Where(t => RemoveAccents(t.TeacherName.ToLower()).Contains(searchNormalized))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchEmail))
            {
                string emailNormalized = searchEmail.ToLower();
                viewModel = viewModel
                    .Where(t => (t.Email ?? "").ToLower().Contains(emailNormalized))
                    .ToList();
            }

            // Ordenamiento
            viewModel = sortBy.ToLower() switch
            {
                "email" => sortOrder == "asc"
                    ? viewModel.OrderBy(t => t.Email ?? "").ToList()
                    : viewModel.OrderByDescending(t => t.Email ?? "").ToList(),
                "alumnos" => sortOrder == "asc"
                    ? viewModel.OrderBy(t => t.AlumnosAsignados).ToList()
                    : viewModel.OrderByDescending(t => t.AlumnosAsignados).ToList(),
                "bitacoras" => sortOrder == "asc"
                    ? viewModel.OrderBy(t => t.TotalBitacoras).ToList()
                    : viewModel.OrderByDescending(t => t.TotalBitacoras).ToList(),
                "pendientes" => sortOrder == "asc"
                    ? viewModel.OrderBy(t => t.BitacorasPendientes).ToList()
                    : viewModel.OrderByDescending(t => t.BitacorasPendientes).ToList(),
                _ => sortOrder == "asc"
                    ? viewModel.OrderBy(t => t.TeacherName).ToList()
                    : viewModel.OrderByDescending(t => t.TeacherName).ToList()
            };

            int totalRecords = viewModel.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginated = viewModel
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchName = searchName;
            ViewBag.SearchEmail = searchEmail;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;

            return View(paginated);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return 0;
            }

            return int.Parse(userIdClaim);
        }

        // GET: Mostrar formulario para agregar alumnos a un asesor (solo Master)
        public async Task<IActionResult> AgregarAlumnosAsesor(int teacherId, string searchName = "", string sortBy = "name", string sortOrder = "asc", string semesterFilter = "", string groupFilter = "", string careerFilter = "", int page = 1, int pageSize = 10)
        {
            if (!User.IsInRole("Master"))
            {
                TempData["Error"] = "Solo un usuario Master puede acceder a esta sección.";
                return RedirectToAction("Maestros");
            }

            // Obtener alumnos que no estén asignados a este maestro (activos)
            var assignedStudentIds = await _context.SocialServiceAssignments
                .Where(a => a.TeacherId == teacherId && a.IsActive)
                .Select(a => a.StudentId)
                .ToListAsync();

            var availableStudentsQuery = _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
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

            var studentIds = availableStudents.Select(s => s.UserId).ToList();
            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => studentIds.Contains(e.StudentId))
                .ToListAsync();

            var viewModel = new List<AvailableStudentViewModel>();
            foreach (var s in availableStudents)
            {
                var enrollment = enrollments.FirstOrDefault(e => e.StudentId == s.UserId);
                var careerName = s.Preenrollments
                    .OrderByDescending(p => p.CreateStat)
                    .Select(p => p.Career != null ? p.Career.name_career : null)
                    .FirstOrDefault();

                viewModel.Add(new AvailableStudentViewModel
                {
                    UserId = s.UserId,
                    FullName = $"{s.Person.LastNamePaternal} {s.Person.LastNameMaternal} {s.Person.FirstName}",
                    Email = s.Email,
                    CareerName = careerName,
                    SemesterName = enrollment?.Group?.GradeLevel?.Name,
                    GroupName = enrollment?.Group?.Name
                });
            }

            // Guardar listas completas para los selects
            var allList = viewModel.ToList();
            var semesters = allList.Where(s => !string.IsNullOrEmpty(s.SemesterName)).Select(s => s.SemesterName).Distinct().OrderBy(s => s).ToList();
            var groups = allList.Where(s => !string.IsNullOrEmpty(s.GroupName)).Select(s => s.GroupName).Distinct().OrderBy(g => g).ToList();
            var careers = allList.Where(s => !string.IsNullOrEmpty(s.CareerName)).Select(s => s.CareerName).Distinct().OrderBy(c => c).ToList();

            // Aplicar filtros
            var filtered = viewModel.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string sn = RemoveAccents(searchName.ToLower());
                filtered = filtered.Where(u => RemoveAccents((u.FullName ?? string.Empty).ToLower()).Contains(sn));
                ViewBag.SearchName = searchName;
            }

            if (!string.IsNullOrWhiteSpace(semesterFilter))
            {
                filtered = filtered.Where(u => u.SemesterName == semesterFilter);
                ViewBag.SemesterFilter = semesterFilter;
            }

            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                filtered = filtered.Where(u => u.GroupName == groupFilter);
                ViewBag.GroupFilter = groupFilter;
            }

            if (!string.IsNullOrWhiteSpace(careerFilter))
            {
                filtered = filtered.Where(u => u.CareerName == careerFilter);
                ViewBag.CareerFilter = careerFilter;
            }

            // Aplicar ordenamiento
            filtered = (sortBy ?? "name").ToLower() switch
            {
                "name" => sortOrder == "asc" ? filtered.OrderBy(u => u.FullName) : filtered.OrderByDescending(u => u.FullName),
                "career" => sortOrder == "asc" ? filtered.OrderBy(u => u.CareerName ?? "") : filtered.OrderByDescending(u => u.CareerName ?? ""),
                "semester" => sortOrder == "asc" ? filtered.OrderBy(u => u.SemesterName ?? "") : filtered.OrderByDescending(u => u.SemesterName ?? ""),
                "group" => sortOrder == "asc" ? filtered.OrderBy(u => u.GroupName ?? "") : filtered.OrderByDescending(u => u.GroupName ?? ""),
                _ => sortOrder == "asc" ? filtered.OrderBy(u => u.FullName) : filtered.OrderByDescending(u => u.FullName),
            };

            // Paginación
            int[] allowedPageSizes = { 10, 25, 50, 100 };
            if (!allowedPageSizes.Contains(pageSize)) pageSize = 10;
            int totalRecords = filtered.Count();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginated = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TeacherId = teacherId;
            ViewBag.Semesters = semesters;
            ViewBag.Groups = groups;
            ViewBag.Careers = careers;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentOrder = sortOrder;
            ViewBag.NextOrder = sortOrder == "asc" ? "desc" : "asc";
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;

            return View(paginated);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarAlumnoAAsesor(int teacherId, int studentId)
        {
            if (!User.IsInRole("Master"))
            {
                TempData["Error"] = "Solo un usuario Master puede realizar esta acción.";
                return RedirectToAction("Maestros");
            }

            var existingAssignment = await _context.SocialServiceAssignments
                .FirstOrDefaultAsync(a => a.TeacherId == teacherId && a.StudentId == studentId);

            if (existingAssignment != null)
            {
                if (!existingAssignment.IsActive)
                {
                    existingAssignment.IsActive = true;
                    existingAssignment.AssignedDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Alumno reasignado al asesor exitosamente.";
                }
                else
                {
                    TempData["Error"] = "Este alumno ya está asignado a este asesor.";
                }

                return RedirectToAction("AgregarAlumnosAsesor", new { teacherId = teacherId });
            }

            var assignment = new social_service_assignment
            {
                TeacherId = teacherId,
                StudentId = studentId,
                AssignedDate = DateTime.Now,
                IsActive = true
            };

            _context.SocialServiceAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Alumno asignado al asesor exitosamente.";
            return RedirectToAction("AgregarAlumnosAsesor", new { teacherId = teacherId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarAsignacionAlumno(int studentId, int teacherId)
        {
            if (!User.IsInRole("Master"))
            {
                TempData["Error"] = "Solo un usuario Master puede realizar esta acción.";
                return RedirectToAction("VerAlumnosAsesor", new { id = teacherId });
            }

            var assignment = await _context.SocialServiceAssignments
                .FirstOrDefaultAsync(a => a.TeacherId == teacherId && a.StudentId == studentId && a.IsActive);

            if (assignment == null)
            {
                TempData["Error"] = "No se encontró la asignación activa para este alumno y maestro.";
                return RedirectToAction("VerAlumnosAsesor", new { id = teacherId });
            }

            assignment.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "El alumno fue eliminado del maestro exitosamente.";
            return RedirectToAction("VerAlumnosAsesor", new { id = teacherId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprobarBitacoraAdmin(int logId, int approvedHoursPracticas, int approvedHoursServicioSocial, string teacherComments)
        {
            if (!User.IsInRole("Master"))
            {
                TempData["Error"] = "No tienes permiso para aprobar esta bitácora.";
                return RedirectToAction("Alumnos");
            }

            var bitacora = await _context.SocialServiceLogs
                .FirstOrDefaultAsync(b => b.LogId == logId);

            if (bitacora == null)
            {
                TempData["Error"] = "Bitácora no encontrada.";
                return RedirectToAction("Alumnos");
            }

            if (approvedHoursPracticas > bitacora.HoursPracticas || approvedHoursServicioSocial > bitacora.HoursServicioSocial)
            {
                TempData["Error"] = "Las horas aprobadas no pueden exceder las horas registradas.";
                return RedirectToAction("VerBitacorasAlumno", new { id = bitacora.StudentId });
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

            // Registrar aprobación
            bitacora.IsApproved = true;
            bitacora.ApprovedHoursPracticas = finalApprovedPracticas;
            bitacora.ApprovedHoursServicioSocial = finalApprovedServicioSocial;
            bitacora.ApprovedBy = GetCurrentUserId();
            bitacora.ApprovedAt = DateTime.Now;
            bitacora.TeacherComments = teacherComments;

            await _context.SaveChangesAsync();

            if (adjustmentMessages.Any())
            {
                string adjustmentInfo = string.Join(" ", adjustmentMessages);
                TempData["Success"] = $"Bitácora aprobada. {adjustmentInfo}";
            }
            else
            {
                TempData["Success"] = "Bitácora aprobada exitosamente. Las horas se han sumado al alumno.";
            }

            return RedirectToAction("VerBitacorasAlumno", new { id = bitacora.StudentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechazarBitacoraAdmin(int logId, string rejectionReason)
        {
            if (!User.IsInRole("Master"))
            {
                TempData["Error"] = "No tienes permiso para rechazar esta bitácora.";
                return RedirectToAction("Alumnos");
            }

            var bitacora = await _context.SocialServiceLogs
                .FirstOrDefaultAsync(b => b.LogId == logId);

            if (bitacora == null)
            {
                TempData["Error"] = "Bitácora no encontrada.";
                return RedirectToAction("Alumnos");
            }

            int studentId = bitacora.StudentId;

            var rejection = new social_service_rejection
            {
                StudentId = bitacora.StudentId,
                Week = bitacora.Week,
                RejectionReason = rejectionReason,
                RejectedBy = GetCurrentUserId(),
                RejectedAt = DateTime.Now
            };

            _context.SocialServiceRejections.Add(rejection);

            _context.SocialServiceLogs.Remove(bitacora);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bitácora rechazada y eliminada. El alumno deberá crear una nueva.";
            return RedirectToAction("VerBitacorasAlumno", new { id = studentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesaprobarBitacoraAdmin(int logId, string adminComments)
        {
            if (!User.IsInRole("Master"))
            {
                TempData["Error"] = "No tienes permiso para desaprobar esta bitácora.";
                return RedirectToAction("Alumnos");
            }

            var bitacora = await _context.SocialServiceLogs
                .FirstOrDefaultAsync(b => b.LogId == logId);

            if (bitacora == null)
            {
                TempData["Error"] = "Bitácora no encontrada.";
                return RedirectToAction("Alumnos");
            }

            if (!bitacora.IsApproved)
            {
                TempData["Error"] = "La bitácora no está aprobada.";
                return RedirectToAction("VerBitacorasAlumno", new { id = bitacora.StudentId });
            }

            // Revertir aprobación: quitar horas aprobadas y datos de aprobación
            bitacora.IsApproved = false;
            bitacora.ApprovedHoursPracticas = 0;
            bitacora.ApprovedHoursServicioSocial = 0;
            bitacora.ApprovedBy = null;
            bitacora.ApprovedAt = null;

            // Agregar comentario indicando quién y por qué se desaprobó
            var currentUserId = GetCurrentUserId();
            var note = string.IsNullOrWhiteSpace(adminComments)
                ? $"[Desaprobada por Master (UserId={currentUserId})]"
                : $"[Desaprobada por Master (UserId={currentUserId}): {adminComments}]";

            bitacora.TeacherComments = string.IsNullOrWhiteSpace(bitacora.TeacherComments)
                ? note
                : bitacora.TeacherComments + " " + note;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Bitácora desaprobada. Las horas aprobadas fueron removidas.";
            return RedirectToAction("VerBitacorasAlumno", new { id = bitacora.StudentId });
        }

        public async Task<IActionResult> Alumnos(string searchName = "", string searchEmail = "", string teacherFilter = "", string careerFilter = "", string semesterFilter = "", string groupFilter = "", string sortBy = "name", string sortOrder = "asc", int page = 1, int pageSize = 10)
        {
            int[] allowedPageSizes = { 10, 25, 50, 100 };
            if (!allowedPageSizes.Contains(pageSize)) pageSize = 10;

            var assignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Include(a => a.Student)
                    .ThenInclude(s => s.Preenrollments)
                        .ThenInclude(p => p.Career)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(a => a.IsActive)
                .ToListAsync();

            var studentIds = assignments.Select(a => a.StudentId).Distinct().ToList();

            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => studentIds.Contains(e.StudentId))
                .ToListAsync();

            var allLogs = await _context.SocialServiceLogs
                .Where(log => studentIds.Contains(log.StudentId))
                .ToListAsync();

            var logsByStudent = allLogs.GroupBy(l => l.StudentId).ToDictionary(g => g.Key, g => g.ToList());

            var viewModel = new List<CoordinatorStudentViewModel>();

            foreach (var assignment in assignments)
            {
                var enrollment = enrollments.FirstOrDefault(e => e.StudentId == assignment.StudentId);
                var studentLogs = logsByStudent.ContainsKey(assignment.StudentId) ? logsByStudent[assignment.StudentId] : new List<social_service_log>();
                var approvedLogs = studentLogs.Where(l => l.IsApproved).ToList();
                var careerName = assignment.Student?.Preenrollments?
                    .OrderByDescending(p => p.CreateStat)
                    .Select(p => p.Career != null ? p.Career.name_career : null)
                    .FirstOrDefault();

                viewModel.Add(new CoordinatorStudentViewModel
                {
                    StudentId = assignment.StudentId,
                    StudentName = $"{assignment.Student.Person.LastNamePaternal} {assignment.Student.Person.LastNameMaternal} {assignment.Student.Person.FirstName}",
                    Email = assignment.Student.Email,
                    CareerName = careerName,
                    SemesterName = enrollment?.Group?.GradeLevel?.Name,
                    GroupName = enrollment?.Group?.Name,
                    TeacherName = assignment.Teacher != null
                        ? $"{assignment.Teacher.Person.LastNamePaternal} {assignment.Teacher.Person.LastNameMaternal} {assignment.Teacher.Person.FirstName}"
                        : "Sin asignar",
                    TotalBitacoras = studentLogs.Count,
                    BitacorasPendientes = studentLogs.Count(l => !l.IsApproved),
                    TotalHorasPracticas = approvedLogs.Sum(l => l.ApprovedHoursPracticas),
                    TotalHorasServicioSocial = approvedLogs.Sum(l => l.ApprovedHoursServicioSocial)
                });
            }

            // Listas para filtros (antes de filtrar)
            var teachersList = viewModel
                .Where(s => !string.IsNullOrEmpty(s.TeacherName))
                .Select(s => s.TeacherName!)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var semesters = viewModel
                .Where(s => !string.IsNullOrEmpty(s.SemesterName))
                .Select(s => s.SemesterName!)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            var careers = viewModel
                .Where(s => !string.IsNullOrEmpty(s.CareerName))
                .Select(s => s.CareerName!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var groups = viewModel
                .Where(s => !string.IsNullOrEmpty(s.GroupName))
                .Select(s => s.GroupName!)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string searchNormalized = RemoveAccents(searchName.ToLower());
                viewModel = viewModel
                    .Where(s => RemoveAccents(s.StudentName.ToLower()).Contains(searchNormalized))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchEmail))
            {
                string emailNormalized = searchEmail.ToLower();
                viewModel = viewModel
                    .Where(s => (s.Email ?? "").ToLower().Contains(emailNormalized))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(teacherFilter))
            {
                viewModel = viewModel.Where(s => s.TeacherName == teacherFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(careerFilter))
            {
                viewModel = viewModel.Where(s => s.CareerName == careerFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(semesterFilter))
            {
                viewModel = viewModel.Where(s => s.SemesterName == semesterFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                viewModel = viewModel.Where(s => s.GroupName == groupFilter).ToList();
            }

            // Ordenamiento
            viewModel = sortBy.ToLower() switch
            {
                "email" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.Email ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.Email ?? "").ToList(),
                "career" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.CareerName ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.CareerName ?? "").ToList(),
                "semester" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.SemesterName ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.SemesterName ?? "").ToList(),
                "group" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.GroupName ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.GroupName ?? "").ToList(),
                "teacher" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.TeacherName ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.TeacherName ?? "").ToList(),
                "bitacoras" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.TotalBitacoras).ToList()
                    : viewModel.OrderByDescending(s => s.TotalBitacoras).ToList(),
                "pendientes" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.BitacorasPendientes).ToList()
                    : viewModel.OrderByDescending(s => s.BitacorasPendientes).ToList(),
                "practicas" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.TotalHorasPracticas).ToList()
                    : viewModel.OrderByDescending(s => s.TotalHorasPracticas).ToList(),
                "servicio" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.TotalHorasServicioSocial).ToList()
                    : viewModel.OrderByDescending(s => s.TotalHorasServicioSocial).ToList(),
                _ => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.StudentName).ToList()
                    : viewModel.OrderByDescending(s => s.StudentName).ToList()
            };

            int totalRecords = viewModel.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginated = viewModel
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchName = searchName;
            ViewBag.SearchEmail = searchEmail;
            ViewBag.TeacherFilter = teacherFilter;
            ViewBag.CareerFilter = careerFilter;
            ViewBag.SemesterFilter = semesterFilter;
            ViewBag.GroupFilter = groupFilter;
            ViewBag.Teachers = teachersList;
            ViewBag.Careers = careers;
            ViewBag.Semesters = semesters;
            ViewBag.Groups = groups;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;

            return View(paginated);
        }

        public async Task<IActionResult> VerBitacorasAlumno(int id, string status = "", string semester = "", string dateFrom = "", string dateTo = "", string sortBy = "created", string sortOrder = "desc")
        {
            var query = _context.SocialServiceLogs
                .Include(b => b.Student)
                    .ThenInclude(s => s.Person)
                .Where(b => b.StudentId == id);

            var allBitacoras = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

            var student = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == id);

            int? teacherId = null;
            if (student != null)
            {
                ViewBag.StudentName = $"{student.Person.LastNamePaternal} {student.Person.LastNameMaternal} {student.Person.FirstName}";
                ViewBag.StudentId = id;
            }

            // Obtener el asesor asignado
            var assignment = await _context.SocialServiceAssignments
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.Person)
                .FirstOrDefaultAsync(a => a.StudentId == id && a.IsActive);

            if (assignment?.Teacher != null)
            {
                ViewBag.TeacherName = $"{assignment.Teacher.Person.LastNamePaternal} {assignment.Teacher.Person.LastNameMaternal} {assignment.Teacher.Person.FirstName}";
                teacherId = assignment.TeacherId;
            }

            // Preparar lista de semestres presentes en las bitácoras para el select
            var semesters = allBitacoras
                .Where(b => !string.IsNullOrEmpty(b.SnapshotSemesterName))
                .Select(b => b.SnapshotSemesterName)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            // Aplicar filtros en memoria
            var filtered = allBitacoras.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "approved") filtered = filtered.Where(b => b.IsApproved);
                else if (status.ToLower() == "pending") filtered = filtered.Where(b => !b.IsApproved);
            }

            if (!string.IsNullOrWhiteSpace(semester))
            {
                filtered = filtered.Where(b => b.SnapshotSemesterName == semester);
            }

            DateTime? from = null, to = null;
            if (DateTime.TryParse(dateFrom, out var pf)) from = pf.Date;
            if (DateTime.TryParse(dateTo, out var pt)) to = pt.Date;
            if (from.HasValue) filtered = filtered.Where(b => b.CreatedAt.Date >= from.Value);
            if (to.HasValue) filtered = filtered.Where(b => b.CreatedAt.Date <= to.Value);

            // Ordenamiento
            filtered = (sortBy ?? "created").ToLower() switch
            {
                "week" => sortOrder == "asc" ? filtered.OrderBy(b => b.Week) : filtered.OrderByDescending(b => b.Week),
                "created" => sortOrder == "asc" ? filtered.OrderBy(b => b.CreatedAt) : filtered.OrderByDescending(b => b.CreatedAt),
                _ => sortOrder == "asc" ? filtered.OrderBy(b => b.CreatedAt) : filtered.OrderByDescending(b => b.CreatedAt),
            };

            ViewBag.Semesters = semesters;
            ViewBag.FilterStatus = status;
            ViewBag.FilterSemester = semester;
            ViewBag.FilterDateFrom = dateFrom;
            ViewBag.FilterDateTo = dateTo;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentOrder = sortOrder;
            ViewBag.TeacherId = teacherId;
            ViewBag.EsCoordinador = User.IsInRole("Coordinator");
            ViewBag.EsMaster = User.IsInRole("Master");

            return View(filtered.ToList());
        }



        public async Task<IActionResult> VerAsistenciasAlumno(int id, string dateFrom = "", string dateTo = "", string status = "", string sortBy = "date", string sortOrder = "desc")
        {
            // Recibe el id del alumno y opcionalmente el id del maestro
            int? teacherId = null;
            // Buscar el assignment activo para obtener el TeacherId
            var assignment = await _context.SocialServiceAssignments
                .FirstOrDefaultAsync(a => a.StudentId == id && a.IsActive);

            if (assignment != null)
            {
                teacherId = assignment.TeacherId;
            }

            var student = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (student == null)
            {
                TempData["Error"] = "Alumno no encontrado.";
                return RedirectToAction("Alumnos");
            }

            var attendances = await _context.SocialServiceAttendances
                .Where(att => att.StudentId == id)
                .ToListAsync();

            // Aplicar filtros por fecha y estado
            DateTime? from = null, to = null;
            if (DateTime.TryParse(dateFrom, out var parsedFrom)) from = parsedFrom.Date;
            if (DateTime.TryParse(dateTo, out var parsedTo)) to = parsedTo.Date;

            var filtered = attendances.AsEnumerable();
            if (from.HasValue)
            {
                filtered = filtered.Where(a => a.Date.Date >= from.Value);
            }
            if (to.HasValue)
            {
                filtered = filtered.Where(a => a.Date.Date <= to.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "present") filtered = filtered.Where(a => a.IsPresent);
                else if (status.ToLower() == "absent") filtered = filtered.Where(a => !a.IsPresent);
            }

            // Aplicar ordenamiento
            filtered = (sortBy ?? "date").ToLower() switch
            {
                "date" => sortOrder == "asc" ? filtered.OrderBy(a => a.Date) : filtered.OrderByDescending(a => a.Date),
                "day" => sortOrder == "asc" ? filtered.OrderBy(a => a.Date.DayOfWeek) : filtered.OrderByDescending(a => a.Date.DayOfWeek),
                "estado" => sortOrder == "asc" ? filtered.OrderBy(a => a.IsPresent) : filtered.OrderByDescending(a => a.IsPresent),
                _ => sortOrder == "asc" ? filtered.OrderBy(a => a.Date) : filtered.OrderByDescending(a => a.Date),
            };

            var viewModel = new CoordinatorAttendanceDetailViewModel
            {
                StudentId = id,
                StudentName = $"{student.Person.LastNamePaternal} {student.Person.LastNameMaternal} {student.Person.FirstName}",
                Attendances = filtered.ToList(),
                TotalPresent = filtered.Count(a => a.IsPresent),
                TotalAbsent = filtered.Count(a => !a.IsPresent)
            };

            // Pasar parámetros de filtro/orden a la vista
            ViewBag.FilterDateFrom = dateFrom;
            ViewBag.FilterDateTo = dateTo;
            ViewBag.FilterStatus = status;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentOrder = sortOrder;

            ViewBag.TeacherId = teacherId;
            ViewBag.EsCoordinador = User.IsInRole("Coordinator");
            ViewBag.EsMaster = User.IsInRole("Master");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAsistencia(int attendanceId, bool isPresent, int studentId)
        {
            // Permitir a Coordinator o Master editar
            if (!(User.IsInRole("Coordinator") || User.IsInRole("Master")))
            {
                TempData["Error"] = "Solo el coordinador o el Master pueden modificar la asistencia.";
                return RedirectToAction("VerAsistenciasAlumno", new { id = studentId });
            }

            var attendance = await _context.SocialServiceAttendances
                .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);

            if (attendance == null)
            {
                TempData["Error"] = "Registro de asistencia no encontrado.";
                return RedirectToAction("VerAsistenciasAlumno", new { id = studentId });
            }

            attendance.IsPresent = isPresent;
            attendance.Notes = $"Editado el {DateTime.Now:dd/MM/yyyy HH:mm}";

            _context.SocialServiceAttendances.Update(attendance);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Asistencia actualizada correctamente.";
            return RedirectToAction("VerAsistenciasAlumno", new { id = studentId });
        }

        public async Task<IActionResult> VerAlumnosAsesor(int id, string searchName = "", string searchEmail = "", string careerFilter = "", string semesterFilter = "", string groupFilter = "", string sortBy = "name", string sortOrder = "asc", int page = 1, int pageSize = 10)
        {
            int[] allowedPageSizes = { 10, 25, 50, 100 };
            if (!allowedPageSizes.Contains(pageSize)) pageSize = 10;

            var teacher = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (teacher == null)
            {
                TempData["Error"] = "Asesor no encontrado.";
                return RedirectToAction("Maestros");
            }

            ViewBag.TeacherName = $"{teacher.Person.LastNamePaternal} {teacher.Person.LastNameMaternal} {teacher.Person.FirstName}";
            ViewBag.TeacherId = id;

            var assignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Include(a => a.Student)
                    .ThenInclude(s => s.Preenrollments)
                        .ThenInclude(p => p.Career)
                .Where(a => a.TeacherId == id && a.IsActive)
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

            var logsByStudent = allLogs.GroupBy(l => l.StudentId).ToDictionary(g => g.Key, g => g.ToList());

            var viewModel = new List<CoordinatorStudentViewModel>();

            foreach (var assignment in assignments)
            {
                var enrollment = enrollments.FirstOrDefault(e => e.StudentId == assignment.StudentId);
                var studentLogs = logsByStudent.ContainsKey(assignment.StudentId) ? logsByStudent[assignment.StudentId] : new List<social_service_log>();
                var approvedLogs = studentLogs.Where(l => l.IsApproved).ToList();
                var careerName = assignment.Student?.Preenrollments?
                    .OrderByDescending(p => p.CreateStat)
                    .Select(p => p.Career != null ? p.Career.name_career : null)
                    .FirstOrDefault();

                viewModel.Add(new CoordinatorStudentViewModel
                {
                    StudentId = assignment.StudentId,
                    StudentName = $"{assignment.Student.Person.LastNamePaternal} {assignment.Student.Person.LastNameMaternal} {assignment.Student.Person.FirstName}",
                    Email = assignment.Student.Email,
                    CareerName = careerName,
                    SemesterName = enrollment?.Group?.GradeLevel?.Name,
                    GroupName = enrollment?.Group?.Name,
                    TotalBitacoras = studentLogs.Count,
                    BitacorasPendientes = studentLogs.Count(l => !l.IsApproved),
                    TotalHorasPracticas = approvedLogs.Sum(l => l.ApprovedHoursPracticas),
                    TotalHorasServicioSocial = approvedLogs.Sum(l => l.ApprovedHoursServicioSocial)
                });
            }

            // Listas para filtros (antes de filtrar)
            var semesters = viewModel
                .Where(s => !string.IsNullOrEmpty(s.SemesterName))
                .Select(s => s.SemesterName!)
                .Distinct().OrderBy(s => s).ToList();

            var careers = viewModel
                .Where(s => !string.IsNullOrEmpty(s.CareerName))
                .Select(s => s.CareerName!)
                .Distinct().OrderBy(c => c).ToList();

            var groups = viewModel
                .Where(s => !string.IsNullOrEmpty(s.GroupName))
                .Select(s => s.GroupName!)
                .Distinct().OrderBy(g => g).ToList();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string searchNormalized = RemoveAccents(searchName.ToLower());
                viewModel = viewModel
                    .Where(s => RemoveAccents(s.StudentName.ToLower()).Contains(searchNormalized))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchEmail))
            {
                string emailNormalized = searchEmail.ToLower();
                viewModel = viewModel
                    .Where(s => (s.Email ?? "").ToLower().Contains(emailNormalized))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(careerFilter))
            {
                viewModel = viewModel.Where(s => s.CareerName == careerFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(semesterFilter))
            {
                viewModel = viewModel.Where(s => s.SemesterName == semesterFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                viewModel = viewModel.Where(s => s.GroupName == groupFilter).ToList();
            }

            // Ordenamiento
            viewModel = sortBy.ToLower() switch
            {
                "email" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.Email ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.Email ?? "").ToList(),
                "career" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.CareerName ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.CareerName ?? "").ToList(),
                "semester" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.SemesterName ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.SemesterName ?? "").ToList(),
                "group" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.GroupName ?? "").ToList()
                    : viewModel.OrderByDescending(s => s.GroupName ?? "").ToList(),
                "bitacoras" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.TotalBitacoras).ToList()
                    : viewModel.OrderByDescending(s => s.TotalBitacoras).ToList(),
                "pendientes" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.BitacorasPendientes).ToList()
                    : viewModel.OrderByDescending(s => s.BitacorasPendientes).ToList(),
                "practicas" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.TotalHorasPracticas).ToList()
                    : viewModel.OrderByDescending(s => s.TotalHorasPracticas).ToList(),
                "servicio" => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.TotalHorasServicioSocial).ToList()
                    : viewModel.OrderByDescending(s => s.TotalHorasServicioSocial).ToList(),
                _ => sortOrder == "asc"
                    ? viewModel.OrderBy(s => s.StudentName).ToList()
                    : viewModel.OrderByDescending(s => s.StudentName).ToList()
            };

            // Totales globales (antes de paginar)
            ViewBag.TotalAlumnos = viewModel.Count;
            ViewBag.TotalBitacoras = viewModel.Sum(s => s.TotalBitacoras);
            ViewBag.TotalPendientes = viewModel.Sum(s => s.BitacorasPendientes);
            ViewBag.TotalHorasAprobadas = viewModel.Sum(s => s.TotalHorasPracticas + s.TotalHorasServicioSocial);

            // Paginación
            int totalRecords = viewModel.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginated = viewModel
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchName = searchName;
            ViewBag.SearchEmail = searchEmail;
            ViewBag.CareerFilter = careerFilter;
            ViewBag.SemesterFilter = semesterFilter;
            ViewBag.GroupFilter = groupFilter;
            ViewBag.Careers = careers;
            ViewBag.Semesters = semesters;
            ViewBag.Groups = groups;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;

            return View(paginated);
        }

        private async Task<List<CoordinatorStudentViewModel>> GetAllStudentsData(string searchName = "", string teacherFilter = "", string careerFilter = "", string semesterFilter = "", string groupFilter = "")
        {
            var assignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
                .Include(a => a.Student)
                    .ThenInclude(s => s.Preenrollments)
                        .ThenInclude(p => p.Career)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(a => a.IsActive)
                .ToListAsync();

            var studentIds = assignments.Select(a => a.StudentId).Distinct().ToList();

            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => studentIds.Contains(e.StudentId))
                .ToListAsync();

            var allLogs = await _context.SocialServiceLogs
                .Where(log => studentIds.Contains(log.StudentId))
                .ToListAsync();

            var logsByStudent = allLogs.GroupBy(l => l.StudentId).ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<CoordinatorStudentViewModel>();

            foreach (var assignment in assignments)
            {
                var enrollment = enrollments.FirstOrDefault(e => e.StudentId == assignment.StudentId);
                var studentLogs = logsByStudent.ContainsKey(assignment.StudentId) ? logsByStudent[assignment.StudentId] : new List<social_service_log>();
                var approvedLogs = studentLogs.Where(l => l.IsApproved).ToList();
                var careerName = assignment.Student?.Preenrollments?
                    .OrderByDescending(p => p.CreateStat)
                    .Select(p => p.Career != null ? p.Career.name_career : null)
                    .FirstOrDefault();

                result.Add(new CoordinatorStudentViewModel
                {
                    StudentId = assignment.StudentId,
                    StudentName = $"{assignment.Student.Person.LastNamePaternal} {assignment.Student.Person.LastNameMaternal} {assignment.Student.Person.FirstName}",
                    Email = assignment.Student.Email,
                    CareerName = careerName,
                    SemesterName = enrollment?.Group?.GradeLevel?.Name,
                    GroupName = enrollment?.Group?.Name,
                    TeacherName = assignment.Teacher != null
                        ? $"{assignment.Teacher.Person.LastNamePaternal} {assignment.Teacher.Person.LastNameMaternal} {assignment.Teacher.Person.FirstName}"
                        : "Sin asignar",
                    TotalBitacoras = studentLogs.Count,
                    BitacorasPendientes = studentLogs.Count(l => !l.IsApproved),
                    TotalHorasPracticas = approvedLogs.Sum(l => l.ApprovedHoursPracticas),
                    TotalHorasServicioSocial = approvedLogs.Sum(l => l.ApprovedHoursServicioSocial)
                });
            }

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string searchNormalized = RemoveAccents(searchName.ToLower());
                result = result
                    .Where(s => RemoveAccents(s.StudentName.ToLower()).Contains(searchNormalized))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(teacherFilter))
            {
                result = result.Where(s => s.TeacherName == teacherFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(careerFilter))
            {
                result = result.Where(s => s.CareerName == careerFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(semesterFilter))
            {
                result = result.Where(s => s.SemesterName == semesterFilter).ToList();
            }

            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                result = result.Where(s => s.GroupName == groupFilter).ToList();
            }

            return result.OrderBy(s => s.StudentName).ToList();
        }

        public async Task<IActionResult> ExportAlumnosExcel(string searchName = "", string teacherFilter = "", string careerFilter = "", string semesterFilter = "", string groupFilter = "")
        {
            var data = await GetAllStudentsData(searchName, teacherFilter, careerFilter, semesterFilter, groupFilter);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Alumnos");

            var headers = new[] { "Alumno", "Carrera", "Semestre", "Grupo", "Asesor", "Total Bitácoras", "Horas Prácticas", "Horas Servicio Social", "Validación" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#8C1B1B");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

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
                ws.Cell(row, 5).Value = alumno.TeacherName;
                ws.Cell(row, 6).Value = alumno.TotalBitacoras;
                ws.Cell(row, 7).Value = $"{alumno.TotalHorasPracticas} / 240";
                ws.Cell(row, 8).Value = $"{alumno.TotalHorasServicioSocial} / 480";
                ws.Cell(row, 9).Value = validacion;

                for (int c = 2; c <= 9; c++)
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Alumnos_Master_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ExportAlumnosPdf(string searchName = "", string teacherFilter = "", string careerFilter = "", string semesterFilter = "", string groupFilter = "")
        {
            var data = await GetAllStudentsData(searchName, teacherFilter, careerFilter, semesterFilter, groupFilter);

            string htmlContent = await this.RenderViewAsync("_AlumnosPdf", data, true);

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
            return File(file, "application/pdf", $"Alumnos_Master_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> DescargarBitacoraPdf(int bitacoraId)
        {
            var bitacora = await _context.SocialServiceLogs.FirstOrDefaultAsync(b => b.LogId == bitacoraId);
            if (bitacora == null || bitacora.PdfFileData == null)
                return NotFound();

            var contentType = !string.IsNullOrEmpty(bitacora.PdfContentType) ? bitacora.PdfContentType : "application/pdf";
            // No se especifica fileDownloadName para que el navegador intente mostrar el PDF en el visor
            return File(bitacora.PdfFileData, contentType);
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
    }
}
