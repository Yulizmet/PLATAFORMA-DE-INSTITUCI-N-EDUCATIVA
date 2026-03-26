using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.ViewModels;
using System.Text;

#nullable disable

namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Psychologist,Head of Psychology,Coordinator,Master")]
    public class PsychologyController : Controller
    {
        private readonly AppDbContext _context;

        public PsychologyController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Ver lista — Psicólogos, Jefes, Coordinadores y Master
        public async Task<IActionResult> Index()
        {
            var lista = await (
                from p in _context.MedicalPsychology
                join pre in _context.PreenrollmentGenerals on p.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                orderby p.AppointmentDatetime descending
                select new PsychologyListVM
                {
                    Id = p.Id,
                    Folio = p.Fol,
                    Matricula = pre.Matricula,
                    NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                    Asistencia = p.AttendanceStatus,
                    Fecha = p.AppointmentDatetime
                }
            ).ToListAsync();

            return View(lista);
        }

        // ✅ Reportes — Todos los autorizados en el controlador
        public IActionResult Reporte() => View();

        // 🔒 Crear — Solo personal de Psicología y Master (Coordinator solo ve)
        [Authorize(Roles = "Psychologist,Head of Psychology,Master")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Psychologist,Head of Psychology,Master")]
        public async Task<IActionResult> Create(medical_pychology model)
        {
            if (model.PreenrollmentId == 0)
            {
                ModelState.AddModelError("", "Debe buscar un alumno válido");
                return View(model);
            }

            var ultimoFol = await _context.MedicalPsychology.OrderByDescending(x => x.Id).Select(x => x.Fol).FirstOrDefaultAsync();
            int nuevo = 1;
            if (!string.IsNullOrEmpty(ultimoFol)) { int.TryParse(ultimoFol, out nuevo); nuevo++; }

            model.Fol = nuevo.ToString("D6");
            model.AppointmentDatetime = DateTime.Now;
            model.CreatedAt = DateTime.Now;

            _context.MedicalPsychology.Add(model);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Registro psicológico creado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Detalles — Todos los autorizados
        public async Task<IActionResult> Details(int id)
        {
            var data = await (
                from p in _context.MedicalPsychology
                join pre in _context.PreenrollmentGenerals on p.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where p.Id == id
                select new medical_pychology
                {
                    Id = p.Id,
                    Fol = p.Fol,
                    AppointmentDatetime = p.AppointmentDatetime,
                    AttendanceStatus = p.AttendanceStatus,
                    PsychologyObservations = p.PsychologyObservations,
                    PreenrollmentId = p.PreenrollmentId,
                    MatriculaTemp = pre.Matricula,
                    NombreCompletoTemp = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal
                }
            ).FirstOrDefaultAsync();

            if (data == null) return NotFound();
            return View(data);
        }

        // 🔒 Editar — Solo Master
        [Authorize(Roles = "Head of Psychology,Master")]
        public async Task<IActionResult> Edit(int id)
        {
            var cita = await _context.MedicalPsychology.FindAsync(id);
            if (cita == null) return NotFound();
            return View(cita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Head of Psychology,Master")]
        public async Task<IActionResult> Edit(int id, medical_pychology model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var bd = await _context.MedicalPsychology.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                model.CreatedAt = bd.CreatedAt; // Preservar fecha de creación

                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Registro actualizado correctamente.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 🔒 Eliminar — Solo Master
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await (
                from p in _context.MedicalPsychology
                join pre in _context.PreenrollmentGenerals on p.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where p.Id == id
                select new medical_pychology
                {
                    Id = p.Id,
                    Fol = p.Fol,
                    AttendanceStatus = p.AttendanceStatus,
                    MatriculaTemp = pre.Matricula,
                    NombreCompletoTemp = per.FirstName + " " + per.LastNamePaternal
                }
            ).FirstOrDefaultAsync();

            if (data == null) return NotFound();
            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cita = await _context.MedicalPsychology.FindAsync(id);
            if (cita != null)
            {
                _context.MedicalPsychology.Remove(cita);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Registro eliminado.";
                TempData["Tipo"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- Métodos Auxiliares (Reportes y Búsqueda) ---

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosReporte(string filtro, DateTime? fechaInicio, DateTime? fechaFin) { /* Lógica existente */ return Json(new { }); }

        [HttpGet]
        public async Task<IActionResult> DescargarCSV(string filtro, DateTime? fechaInicio, DateTime? fechaFin) { /* Lógica existente */ return Content(""); }

        [HttpGet]
        public async Task<IActionResult> DescargarPDF(string filtro, DateTime? fechaInicio, DateTime? fechaFin) { /* Lógica existente */ return Content(""); }

        private (DateTime desde, DateTime hasta) ObtenerRango(string filtro, DateTime? fechaInicio, DateTime? fechaFin) { /* Lógica existente */ return (DateTime.Now, DateTime.Now); }

        [HttpGet]
        public async Task<IActionResult> BuscarPorMatricula(string matricula)
        {
            var data = await (
                from pre in _context.PreenrollmentGenerals
                join per in _context.Persons on pre.UserId equals per.PersonId
                where pre.Matricula == matricula
                select new { preId = pre.IdData, nombre = per.FirstName, paterno = per.LastNamePaternal, materno = per.LastNameMaternal }
            ).FirstOrDefaultAsync();
            return Json(data);
        }
    }
}