using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System.Security.Claims;
using SchoolManager.Data;
using SchoolManager.Areas.SocialService.ViewModels;
using SchoolManager.Models;

namespace SchoolManager.Areas.SocialService.Controllers
{
    [Area("SocialService")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }
        public IActionResult Dashboard()
        {
            int currentStudentId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            if (currentStudentId == 0)
            {
                TempData["Error"] = "No se pudo identificar al usuario.";
                return RedirectToAction("Login", "Account", new { area = "UserMng" });
            }

            // Verificar que el estudiante esté asignado a un maestro
            if (!IsStudentAssigned(currentStudentId))
            {
                TempData["Error"] = "No tienes acceso a Servicio Social. Debes estar asignado a un maestro asesor.";
                return RedirectToAction("SistemaEscolar", "MainScreen", new { area = "MainScreen" });
            }

            var approvedLogs = _context.SocialServiceLogs
                .Where(log => log.StudentId == currentStudentId && log.IsApproved)
                .ToList();

            int totalHoursPracticas = approvedLogs.Sum(log => log.ApprovedHoursPracticas);
            int totalHoursServicioSocial = approvedLogs.Sum(log => log.ApprovedHoursServicioSocial);

            int requiredHoursPracticas = 240;
            int requiredHoursServicioSocial = 480;

            // Obtener el nombre del asesor asignado
            var assignment = _context.SocialServiceAssignments
                .Where(a => a.StudentId == currentStudentId && a.IsActive)
                .Select(a => new
                {
                    TeacherFirstName = a.Teacher.Person.FirstName,
                    TeacherLastNamePaternal = a.Teacher.Person.LastNamePaternal,
                    TeacherLastNameMaternal = a.Teacher.Person.LastNameMaternal
                })
                .FirstOrDefault();

            string teacherName = "Sin asignar";
            if (assignment != null)
            {
                teacherName = $"{assignment.TeacherFirstName} {assignment.TeacherLastNamePaternal} {assignment.TeacherLastNameMaternal}";
            }

            ViewBag.TotalHoursPracticas = totalHoursPracticas;
            ViewBag.RemainingHoursPracticas = Math.Max(0, requiredHoursPracticas - totalHoursPracticas);
            ViewBag.TotalHoursServicioSocial = totalHoursServicioSocial;
            ViewBag.RemainingHoursServicioSocial = Math.Max(0, requiredHoursServicioSocial - totalHoursServicioSocial);
            ViewBag.TeacherName = teacherName;

            return View();
        }

        [HttpGet]
        public IActionResult Download(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return BadRequest();

            filename = Path.GetFileName(filename);

            var areaUploads = Path.Combine(Directory.GetCurrentDirectory(), "Areas", "SocialService", "uploads");
            var areaPath = Path.Combine(areaUploads, filename);

            var wwwrootUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "servicio-social");
            var wwwrootPath = Path.Combine(wwwrootUploads, filename);

            string fullPath = null;
            if (System.IO.File.Exists(areaPath))
            {
                fullPath = areaPath;
            }
            else if (System.IO.File.Exists(wwwrootPath))
            {
                fullPath = wwwrootPath;
            }

            if (fullPath == null)
                return NotFound();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullPath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var stream = System.IO.File.OpenRead(fullPath);
            return File(stream, contentType, fileDownloadName: filename);
        }

        public IActionResult Bitacoras()
        {
            int currentStudentId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            if (currentStudentId == 0)
            {
                TempData["Error"] = "No se pudo identificar al usuario.";
                return RedirectToAction("Login", "Account", new { area = "UserMng" });
            }

            if (!IsStudentAssigned(currentStudentId))
            {
                TempData["Error"] = "No tienes acceso a Servicio Social. Debes estar asignado a un maestro asesor.";
                return RedirectToAction("SistemaEscolar", "MainScreen", new { area = "MainScreen" });
            }

            var bitacoras = _context.SocialServiceLogs
                .Where(x => x.StudentId == currentStudentId)
                .OrderByDescending(x => x.Week)
                .ToList();

            return View(bitacoras);
        }

        public IActionResult CrearBitacora()
        {
            int currentStudentId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            if (currentStudentId == 0)
            {
                TempData["Error"] = "No se pudo identificar al usuario.";
                return RedirectToAction("Login", "Account", new { area = "UserMng" });
            }

            if (!IsStudentAssigned(currentStudentId))
            {
                TempData["Error"] = "No tienes acceso a Servicio Social. Debes estar asignado a un maestro asesor.";
                return RedirectToAction("SistemaEscolar", "MainScreen", new { area = "MainScreen" });
            }

            return View();
        }

        [HttpPost]
        public IActionResult CrearBitacora(BitacoraViewModel vm)
        {
            if (ModelState.IsValid)
            {
                int currentStudentId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                if (currentStudentId == 0)
                {
                    TempData["Error"] = "No se pudo identificar al usuario.";
                    return RedirectToAction("Login", "Account", new { area = "UserMng" });
                }

                if (!IsStudentAssigned(currentStudentId))
                {
                    TempData["Error"] = "No tienes acceso a Servicio Social. Debes estar asignado a un maestro asesor.";
                    return RedirectToAction("SistemaEscolar", "MainScreen", new { area = "MainScreen" });
                }

                var existingLog = _context.SocialServiceLogs
                    .FirstOrDefault(log => log.StudentId == currentStudentId && log.Week == vm.Week);

                if (existingLog != null)
                {
                    TempData["Error"] = $"Ya tienes una bitácora registrada para la {vm.Week}. No puedes crear más de una bitácora por semana.";
                    return View(vm);
                }

                var log = new social_service_log
                {
                    StudentId = currentStudentId,
                    Week = vm.Week,
                    Activities = vm.Activities,
                    HoursPracticas = vm.HoursPracticas,
                    HoursServicioSocial = vm.HoursServicioSocial,
                    Observations = vm.Observations,
                    CreatedAt = DateTime.Now
                };
                _context.SocialServiceLogs.Add(log);
                _context.SaveChanges();
                TempData["Success"] = "Bitácora registrada exitosamente.";
                return RedirectToAction("Bitacoras");
            }
            return View(vm);
        }

        public IActionResult Horas()
        {
            int currentStudentId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            if (currentStudentId == 0)
            {
                TempData["Error"] = "No se pudo identificar al usuario.";
                return RedirectToAction("Login", "Account", new { area = "UserMng" });
            }

            if (!IsStudentAssigned(currentStudentId))
            {
                TempData["Error"] = "No tienes acceso a Servicio Social. Debes estar asignado a un maestro asesor.";
                return RedirectToAction("SistemaEscolar", "MainScreen", new { area = "MainScreen" });
            }

            var approvedLogs = _context.SocialServiceLogs
                .Where(log => log.StudentId == currentStudentId && log.IsApproved)
                .ToList();

            int totalHoursPracticas = approvedLogs.Sum(log => log.ApprovedHoursPracticas);
            int totalHoursServicioSocial = approvedLogs.Sum(log => log.ApprovedHoursServicioSocial);

            // TODO: Las horas requeridas deberían venir de configuración
            int requiredHoursPracticas = 240;
            int requiredHoursServicioSocial = 480;

            ViewBag.TotalHoursPracticas = totalHoursPracticas;
            ViewBag.RequiredHoursPracticas = requiredHoursPracticas;
            ViewBag.RemainingHoursPracticas = Math.Max(0, requiredHoursPracticas - totalHoursPracticas);
            ViewBag.PercentagePracticas = requiredHoursPracticas > 0
                ? (int)((double)totalHoursPracticas / requiredHoursPracticas * 100)
                : 0;

            ViewBag.TotalHoursServicioSocial = totalHoursServicioSocial;
            ViewBag.RequiredHoursServicioSocial = requiredHoursServicioSocial;
            ViewBag.RemainingHoursServicioSocial = Math.Max(0, requiredHoursServicioSocial - totalHoursServicioSocial);
            ViewBag.PercentageServicioSocial = requiredHoursServicioSocial > 0
                ? (int)((double)totalHoursServicioSocial / requiredHoursServicioSocial * 100)
                : 0;

            return View();
        }

        private bool IsStudentAssigned(int studentId)
        {
            return _context.SocialServiceAssignments
                .Any(a => a.StudentId == studentId && a.IsActive);
        }
    }
}