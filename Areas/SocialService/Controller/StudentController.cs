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
                .ToList()
                .OrderByDescending(x => ExtractWeekNumber(x.Week))
                .ToList();

            var rejections = _context.SocialServiceRejections
                .Where(r => r.StudentId == currentStudentId && !r.IsRead)
                .OrderByDescending(r => r.RejectedAt)
                .ToList();

            ViewBag.Rejections = rejections;

            return View(bitacoras);
        }

        [HttpPost]
        public IActionResult MarcarRechazoLeido(int rejectionId)
        {
            var rejection = _context.SocialServiceRejections.FirstOrDefault(r => r.RejectionId == rejectionId);
            if (rejection != null)
            {
                rejection.IsRead = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Bitacoras");
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

            var approvedLogs = _context.SocialServiceLogs
                .Where(log => log.StudentId == currentStudentId && log.IsApproved)
                .ToList();

            int totalHoursPracticas = approvedLogs.Sum(log => log.ApprovedHoursPracticas);
            int totalHoursServicioSocial = approvedLogs.Sum(log => log.ApprovedHoursServicioSocial);

            const int requiredHoursPracticas = 240;
            const int requiredHoursServicioSocial = 480;

            if (totalHoursPracticas >= requiredHoursPracticas && totalHoursServicioSocial >= requiredHoursServicioSocial)
            {
                TempData["Success"] = "¡Felicidades! Ya cumpliste con tu servicio social. No puedes crear más bitácoras.";
                return RedirectToAction("Bitacoras");
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

                // Verificar si ya completó todas las horas requeridas
                var approvedLogs = _context.SocialServiceLogs
                    .Where(log => log.StudentId == currentStudentId && log.IsApproved)
                    .ToList();

                int totalHoursPracticas = approvedLogs.Sum(log => log.ApprovedHoursPracticas);
                int totalHoursServicioSocial = approvedLogs.Sum(log => log.ApprovedHoursServicioSocial);

                const int requiredHoursPracticas = 240;
                const int requiredHoursServicioSocial = 480;

                if (totalHoursPracticas >= requiredHoursPracticas && totalHoursServicioSocial >= requiredHoursServicioSocial)
                {
                    TempData["Error"] = "Ya cumpliste con tu servicio social. No puedes crear más bitácoras.";
                    return RedirectToAction("Bitacoras");
                }

                int remainingHoursPracticas = requiredHoursPracticas - totalHoursPracticas;
                int remainingHoursServicioSocial = requiredHoursServicioSocial - totalHoursServicioSocial;

                if (vm.HoursPracticas > remainingHoursPracticas)
                {
                    TempData["Warning"] = $"Solo te faltan {remainingHoursPracticas} horas de Prácticas Profesionales. Has registrado {vm.HoursPracticas} horas, pero solo se contabilizarán {remainingHoursPracticas} horas.";
                }

                if (vm.HoursServicioSocial > remainingHoursServicioSocial)
                {
                    TempData["Warning"] = $"Solo te faltan {remainingHoursServicioSocial} horas de Servicio Social. Has registrado {vm.HoursServicioSocial} horas, pero solo se contabilizarán {remainingHoursServicioSocial} horas.";
                }

                var existingLog = _context.SocialServiceLogs
                    .FirstOrDefault(log => log.StudentId == currentStudentId && log.Week == vm.Week);

                if (existingLog != null)
                {
                    TempData["Error"] = $"Ya tienes una bitácora registrada para la Semana {vm.Week}. No puedes crear más de una bitácora por semana.";
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

                // Guardar archivo PDF si se proporcionó
                if (vm.PdfFile != null && vm.PdfFile.Length > 0)
                {
                    var pdfData = ValidateAndReadPdfFile(vm.PdfFile);
                    if (pdfData == null)
                    {
                        TempData["Error"] = "Solo se permiten archivos PDF de máximo 10 MB.";
                        return View(vm);
                    }
                    log.PdfFileName = vm.PdfFile.FileName;
                    log.PdfFileData = pdfData;
                    log.PdfContentType = "application/pdf";
                }

                _context.SocialServiceLogs.Add(log);
                _context.SaveChanges();
                TempData["Success"] = "Bitácora registrada exitosamente.";
                return RedirectToAction("Bitacoras");
            }
            return View(vm);
        }

        [HttpGet]
        public IActionResult EditarBitacora(int id)
        {
            int currentStudentId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            if (currentStudentId == 0)
            {
                TempData["Error"] = "No se pudo identificar al usuario.";
                return RedirectToAction("Login", "Account", new { area = "UserMng" });
            }

            var bitacora = _context.SocialServiceLogs
                .FirstOrDefault(log => log.LogId == id && log.StudentId == currentStudentId);

            if (bitacora == null)
            {
                TempData["Error"] = "Bitácora no encontrada.";
                return RedirectToAction("Bitacoras");
            }

            if (bitacora.IsApproved)
            {
                TempData["Error"] = "No puedes editar una bitácora que ya ha sido aprobada.";
                return RedirectToAction("Bitacoras");
            }

            var viewModel = new BitacoraViewModel
            {
                Week = bitacora.Week,
                Activities = bitacora.Activities,
                HoursPracticas = bitacora.HoursPracticas,
                HoursServicioSocial = bitacora.HoursServicioSocial,
                Observations = bitacora.Observations,
                ExistingPdfFileName = bitacora.PdfFileName
            };

            ViewBag.LogId = bitacora.LogId;
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult EditarBitacora(int id, BitacoraViewModel vm)
        {
            if (ModelState.IsValid)
            {
                int currentStudentId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                if (currentStudentId == 0)
                {
                    TempData["Error"] = "No se pudo identificar al usuario.";
                    return RedirectToAction("Login", "Account", new { area = "UserMng" });
                }

                var bitacora = _context.SocialServiceLogs
                    .FirstOrDefault(log => log.LogId == id && log.StudentId == currentStudentId);

                if (bitacora == null)
                {
                    TempData["Error"] = "Bitácora no encontrada.";
                    return RedirectToAction("Bitacoras");
                }

                if (bitacora.IsApproved)
                {
                    TempData["Error"] = "No puedes editar una bitácora que ya ha sido aprobada.";
                    return RedirectToAction("Bitacoras");
                }

                if (bitacora.Week != vm.Week)
                {
                    var existingWeek = _context.SocialServiceLogs
                        .Any(log => log.StudentId == currentStudentId && log.Week == vm.Week && log.LogId != id);

                    if (existingWeek)
                    {
                        TempData["Error"] = $"Ya existe otra bitácora para {vm.Week}.";
                        ViewBag.LogId = bitacora.LogId;
                        return View(vm);
                    }
                }

                bitacora.Week = vm.Week;
                bitacora.Activities = vm.Activities;
                bitacora.HoursPracticas = vm.HoursPracticas;
                bitacora.HoursServicioSocial = vm.HoursServicioSocial;
                bitacora.Observations = vm.Observations;

                // Guardar archivo PDF si se proporcionó uno nuevo
                if (vm.PdfFile != null && vm.PdfFile.Length > 0)
                {
                    var pdfData = ValidateAndReadPdfFile(vm.PdfFile);
                    if (pdfData == null)
                    {
                        TempData["Error"] = "Solo se permiten archivos PDF de máximo 10 MB.";
                        ViewBag.LogId = id;
                        return View(vm);
                    }
                    bitacora.PdfFileName = vm.PdfFile.FileName;
                    bitacora.PdfFileData = pdfData;
                    bitacora.PdfContentType = "application/pdf";
                }

                _context.SaveChanges();
                TempData["Success"] = "Bitácora actualizada exitosamente.";
                return RedirectToAction("Bitacoras");
            }

            ViewBag.LogId = id;
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

        private byte[]? ValidateAndReadPdfFile(IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
                return null;

            // Validar que sea PDF
            var extension = Path.GetExtension(pdfFile.FileName).ToLowerInvariant();
            if (extension != ".pdf")
                return null;

            // Validar tamaño máximo de 10 MB
            if (pdfFile.Length > 10 * 1024 * 1024)
                return null;

            using var ms = new MemoryStream();
            pdfFile.CopyTo(ms);
            return ms.ToArray();
        }

        [HttpGet]
        public IActionResult DescargarBitacoraPdf(int logId)
        {
            var bitacora = _context.SocialServiceLogs.FirstOrDefault(b => b.LogId == logId);

            if (bitacora == null || string.IsNullOrEmpty(bitacora.PdfFileName))
                return NotFound();

            int currentStudentId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            // Permitir al estudiante dueño de la bitácora
            bool isOwner = bitacora.StudentId == currentStudentId;

            // Permitir al maestro asignado
            bool isTeacher = _context.SocialServiceAssignments
                .Any(a => a.TeacherId == currentStudentId && a.StudentId == bitacora.StudentId && a.IsActive);

            if (!isOwner && !isTeacher)
                return Forbid();

            // Servir desde la base de datos
            if (bitacora.PdfFileData != null && bitacora.PdfFileData.Length > 0)
            {
                return File(bitacora.PdfFileData, bitacora.PdfContentType ?? "application/pdf", bitacora.PdfFileName);
            }

            // Fallback: intentar leer del filesystem para archivos anteriores
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bitacoras", bitacora.PdfFileName);
            if (System.IO.File.Exists(filePath))
            {
                var stream = System.IO.File.OpenRead(filePath);
                return File(stream, "application/pdf", bitacora.PdfFileName);
            }

            return NotFound();
        }
    }
}