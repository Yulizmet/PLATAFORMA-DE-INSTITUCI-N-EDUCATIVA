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

namespace SchoolManager.Areas.Gestion.Controllers
{
    [Area("Gestion")]
    [Authorize]
    public class TemasTutoriasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private int LoggedUserId => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        private bool IsAdmin => User.IsInRole("Administrator") || User.IsInRole("Master");

        public TemasTutoriasController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin) return Content("Acceso denegado. Solo administradores.");

            var temas = await _context.TutorshipSuggestedTopics
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            return View("~/Areas/Tutorship/Views/TemasTutorias.cshtml", temas);
        }

        [HttpPost]
        public async Task<IActionResult> CrearTema(string titulo, string descripcion, DateTime fechaInicio, DateTime fechaFin, IFormFile ArchivoAdjunto)
        {
            if (!IsAdmin) return Content("Acceso denegado.");

            string rutaArchivo = "Sin archivo";

            if (ArchivoAdjunto != null && ArchivoAdjunto.Length > 0)
            {
                string carpetaUploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "temas_tutorias");
                if (!Directory.Exists(carpetaUploads)) Directory.CreateDirectory(carpetaUploads);

                string nombreArchivoUnico = Guid.NewGuid().ToString() + "_" + ArchivoAdjunto.FileName;
                string rutaFisica = Path.Combine(carpetaUploads, nombreArchivoUnico);

                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await ArchivoAdjunto.CopyToAsync(stream);
                }
                rutaArchivo = "/uploads/temas_tutorias/" + nombreArchivoUnico;
            }

            var nuevoTema = new tutorship_suggested_topic
            {
                Title = titulo,
                Description = descripcion,
                StartDate = fechaInicio,
                EndDate = fechaFin,
                FilePath = rutaArchivo,
                CreatedBy = LoggedUserId,
                CreatedAt = DateTime.Now
            };

            _context.TutorshipSuggestedTopics.Add(nuevoTema);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Tema sugerido creado correctamente.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> EditarTema(int id, string titulo, string descripcion, DateTime fechaInicio, DateTime fechaFin)
        {
            if (!IsAdmin) return Content("Acceso denegado.");

            var tema = await _context.TutorshipSuggestedTopics.FindAsync(id);
            if (tema != null)
            {
                tema.Title = titulo;
                tema.Description = descripcion;
                tema.StartDate = fechaInicio;
                tema.EndDate = fechaFin;

                await _context.SaveChangesAsync();
                TempData["Exito"] = "Tema modificado correctamente.";
            }

            return RedirectToAction("Index"); 
        }

        [HttpPost]
        public async Task<IActionResult> EliminarTema(int id)
        {
            if (!IsAdmin) return Content("Acceso denegado.");

            var tema = await _context.TutorshipSuggestedTopics.FindAsync(id);
            if (tema != null)
            {
                var asistenciasLigadas = await _context.TutorshipAttendances.Where(a => a.SuggestedTopicId == id).ToListAsync();
                foreach (var asistencia in asistenciasLigadas)
                {
                    asistencia.SuggestedTopicId = null;
                }

                if (tema.FilePath != "Sin archivo")
                {
                    string rutaFisica = _webHostEnvironment.WebRootPath + tema.FilePath;
                    if (System.IO.File.Exists(rutaFisica)) System.IO.File.Delete(rutaFisica);
                }

                _context.TutorshipSuggestedTopics.Remove(tema);
                await _context.SaveChangesAsync();

                TempData["Exito"] = "Tema eliminado correctamente.";
            }

            return RedirectToAction("Index");
        }
    }
}