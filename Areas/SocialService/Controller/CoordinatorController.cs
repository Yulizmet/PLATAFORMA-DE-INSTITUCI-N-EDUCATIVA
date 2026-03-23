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

            var teacherUserIds = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.Name == "Teacher" && ur.IsActive)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var teachers = await _context.Users
                .Include(u => u.Person)
                .Where(u => teacherUserIds.Contains(u.UserId) && u.IsActive)
                .ToListAsync();

            var assignments = await _context.SocialServiceAssignments
                .Where(a => a.IsActive)
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

        public async Task<IActionResult> Alumnos(string searchName = "", string searchEmail = "", string teacherFilter = "", string semesterFilter = "", string groupFilter = "", string sortBy = "name", string sortOrder = "asc", int page = 1, int pageSize = 10)
        {
            int[] allowedPageSizes = { 10, 25, 50, 100 };
            if (!allowedPageSizes.Contains(pageSize)) pageSize = 10;

            var assignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
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

                viewModel.Add(new CoordinatorStudentViewModel
                {
                    StudentId = assignment.StudentId,
                    StudentName = $"{assignment.Student.Person.LastNamePaternal} {assignment.Student.Person.LastNameMaternal} {assignment.Student.Person.FirstName}",
                    Email = assignment.Student.Email,
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
            ViewBag.SemesterFilter = semesterFilter;
            ViewBag.GroupFilter = groupFilter;
            ViewBag.Teachers = teachersList;
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

        public async Task<IActionResult> VerBitacorasAlumno(int id)
        {
            var bitacoras = await _context.SocialServiceLogs
                .Include(b => b.Student)
                    .ThenInclude(s => s.Person)
                .Where(b => b.StudentId == id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

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

            ViewBag.TeacherId = teacherId;
            return View(bitacoras);
        }

        public async Task<IActionResult> VerAsistenciasAlumno(int id)
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
                .OrderByDescending(att => att.Date)
                .ToListAsync();

            var viewModel = new CoordinatorAttendanceDetailViewModel
            {
                StudentId = id,
                StudentName = $"{student.Person.LastNamePaternal} {student.Person.LastNameMaternal} {student.Person.FirstName}",
                Attendances = attendances,
                TotalPresent = attendances.Count(a => a.IsPresent),
                TotalAbsent = attendances.Count(a => !a.IsPresent)
            };

            ViewBag.TeacherId = teacherId;
            ViewBag.EsCoordinador = User.IsInRole("Coordinator");
            ViewBag.EsMaster = User.IsInRole("Master");
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditarAsistencia(int attendanceId, bool isPresent, int studentId)
        {
            // Solo permitir a Coordinador editar
            if (!User.IsInRole("Coordinator"))
            {
                TempData["Error"] = "Solo el coordinador puede modificar la asistencia.";
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

        public async Task<IActionResult> VerAlumnosAsesor(int id, string searchName = "", string searchEmail = "", string semesterFilter = "", string groupFilter = "", string sortBy = "name", string sortOrder = "asc", int page = 1, int pageSize = 10)
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

                viewModel.Add(new CoordinatorStudentViewModel
                {
                    StudentId = assignment.StudentId,
                    StudentName = $"{assignment.Student.Person.LastNamePaternal} {assignment.Student.Person.LastNameMaternal} {assignment.Student.Person.FirstName}",
                    Email = assignment.Student.Email,
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
            ViewBag.SemesterFilter = semesterFilter;
            ViewBag.GroupFilter = groupFilter;
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

        private async Task<List<CoordinatorStudentViewModel>> GetAllStudentsData(String searchName = "", string teacherFilter = "", string semesterFilter = "", string groupFilter = "")
        {
            var assignments = await _context.SocialServiceAssignments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Person)
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

                result.Add(new CoordinatorStudentViewModel
                {
                    StudentId = assignment.StudentId,
                    StudentName = $"{assignment.Student.Person.LastNamePaternal} {assignment.Student.Person.LastNameMaternal} {assignment.Student.Person.FirstName}",
                    Email = assignment.Student.Email,
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

        public async Task<IActionResult> ExportAlumnosExcel(string searchName = "", string teacherFilter = "", string semesterFilter = "", string groupFilter = "")
        {
            var data = await GetAllStudentsData(searchName, teacherFilter, semesterFilter, groupFilter);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Alumnos");

            var headers = new[] { "Alumno", "Semestre", "Grupo", "Asesor", "Total Bitácoras", "Bitácoras Pendientes", "Horas Prácticas", "Horas Servicio Social", "Validación" };
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
                ws.Cell(row, 2).Value = alumno.SemesterName ?? "N/A";
                ws.Cell(row, 3).Value = alumno.GroupName ?? "N/A";
                ws.Cell(row, 4).Value = alumno.TeacherName;
                ws.Cell(row, 5).Value = alumno.TotalBitacoras;
                ws.Cell(row, 6).Value = alumno.BitacorasPendientes;
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

        public async Task<IActionResult> ExportAlumnosPdf(string searchName = "", string teacherFilter = "", string semesterFilter = "", string groupFilter = "")
        {
            var data = await GetAllStudentsData(searchName, teacherFilter, semesterFilter, groupFilter);

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
