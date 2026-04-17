using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SchoolManager.Areas.Tutorship.Controllers
{
    [Area("Gestion")]
    [Authorize]
    public class SeguimientoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private int LoggedUserId => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        private string LoggedRoleName => User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Ninguno";

        public SeguimientoController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult AccesoDenegado()
        {
            return Content("No tienes permiso para ver esta pantalla. Tu rol actual es: " + LoggedRoleName);
        }

        public async Task<IActionResult> Seguimiento(string matriculaBuscar)
        {
            if (!User.IsInRole("Teacher") && !User.IsInRole("Administrator") && !User.IsInRole("Master"))
            {
                return RedirectToAction("AccesoDenegado", "Tutorship");
            }

            if (string.IsNullOrEmpty(matriculaBuscar))
            {
                return View("~/Areas/Tutorship/Views/Seguimiento.cshtml");
            }

            var preinscripcion = await _context.PreenrollmentGenerals
                .Where(p => p.Matricula == matriculaBuscar)
                .Select(p => new { UserId = p.UserId, Matricula = p.Matricula })
                .FirstOrDefaultAsync();

            if (preinscripcion == null || preinscripcion.UserId == null)
            {
                TempData["Error"] = "No se encontró ningún alumno con la matrícula: " + matriculaBuscar;
                return View("~/Areas/Tutorship/Views/Seguimiento.cshtml");
            }

            var alumno = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == preinscripcion.UserId);

            if (alumno == null)
            {
                TempData["Error"] = "El alumno existe pero no tiene datos personales registrados.";
                return View("~/Areas/Tutorship/Views/Seguimiento.cshtml");
            }

            if (User.IsInRole("Teacher"))
            {
                bool esTutor = await _context.Tutorships.AnyAsync(t => t.StudentId == alumno.UserId && t.TeacherId == LoggedUserId);
                if (!esTutor)
                {
                    TempData["Error"] = "Acceso denegado: Puedes ver que el alumno existe, pero no pertenece a tu grupo de tutoría para dejar reportes.";
                    return View("~/Areas/Tutorship/Views/Seguimiento.cshtml");
                }
            }

            var historial = await _context.TutorshipMonitorings
                .Where(m => m.StudentId == alumno.UserId)
                .OrderByDescending(m => m.Date)
                .ToListAsync();

            ViewBag.Matricula = preinscripcion.Matricula;
            ViewBag.Historial = historial;

            var bitacorasPsicologo = await (
                from p in _context.MedicalPsychology
                join pre in _context.PreenrollmentGenerals on p.PreenrollmentId equals pre.IdData
                where pre.Matricula == preinscripcion.Matricula
                select new
                {
                    Id = p.Id,
                    Fol = p.Fol,
                    AppointmentDatetime = p.AppointmentDatetime,
                    AttendanceStatus = p.AttendanceStatus,
                    PsychologyObservations = p.PsychologyObservations
                }
            ).OrderByDescending(x => x.AppointmentDatetime).ToListAsync();

            ViewBag.BitacorasPsicologia = bitacorasPsicologo;

            var bitacorasClinicas = await (
                from b in _context.MedicalLogbooks
                join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                where pre.Matricula == preinscripcion.Matricula
                select new
                {
                    Folio = b.Folio,
                    FechaHora = b.FechaHora,
                    Estado = b.Estado,
                    MotivoConsulta = b.MotivoConsulta,
                    Tratamiento = b.Tratamiento
                }
            ).OrderByDescending(x => x.FechaHora).ToListAsync();

            ViewBag.BitacorasClinicas = bitacorasClinicas;

            return View("~/Areas/Tutorship/Views/Seguimiento.cshtml", alumno);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarSeguimiento(int studentId, string matricula, string tipo, string observaciones, IFormFile ArchivoAdjunto)
        {
            if (!User.IsInRole("Teacher") && !User.IsInRole("Administrator") && !User.IsInRole("Master"))
            {
                return RedirectToAction("AccesoDenegado", "Tutorship");
            }

            string rutaArchivoBaseDeDatos = "Sin archivo";

            if (ArchivoAdjunto != null && ArchivoAdjunto.Length > 0)
            {
                string carpetaUploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "seguimiento");

                if (!Directory.Exists(carpetaUploads))
                {
                    Directory.CreateDirectory(carpetaUploads);
                }

                string nombreArchivoUnico = Guid.NewGuid().ToString() + "_" + ArchivoAdjunto.FileName;
                string rutaFisicaCompleta = Path.Combine(carpetaUploads, nombreArchivoUnico);

                using (var stream = new FileStream(rutaFisicaCompleta, FileMode.Create))
                {
                    await ArchivoAdjunto.CopyToAsync(stream);
                }

                rutaArchivoBaseDeDatos = "/uploads/seguimiento/" + nombreArchivoUnico;
            }

            var nuevoReporte = new tutorship_monitoring
            {
                StudentId = studentId,
                TeacherId = LoggedUserId,
                Date = DateTime.Now,
                PerformanceLevel = tipo ?? "General",
                DetailedObservations = observaciones ?? "Sin observaciones",
                ActionPlan = "N/A",
                FilePath = rutaArchivoBaseDeDatos
            };

            _context.TutorshipMonitorings.Add(nuevoReporte);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Reporte guardado correctamente.";

            return RedirectToAction("Seguimiento", new { matriculaBuscar = matricula });
        }
    }
} 