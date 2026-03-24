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
    [Authorize(Roles = "Nurse,Psychologist,Head Nurse,Head of Psychology,Coordinator,Master")]
    public class PsychologyController : Controller
    {
        private readonly AppDbContext _context;

        public PsychologyController(AppDbContext context)
        {
            _context = context;
        }

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

        public IActionResult Reporte() => View();

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosReporte(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var (desde, hasta) = ObtenerRango(filtro, fechaInicio, fechaFin);

            var datos = await (
                from p in _context.MedicalPsychology
                join pre in _context.PreenrollmentGenerals on p.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where p.AppointmentDatetime >= desde && p.AppointmentDatetime < hasta
                select new PsychologyListVM
                {
                    Id = p.Id,
                    Folio = p.Fol,
                    Matricula = pre.Matricula,
                    NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                    Asistencia = p.AttendanceStatus,
                    Fecha = p.AppointmentDatetime
                }
            ).OrderByDescending(x => x.Fecha).ToListAsync();

            var resumen = new
            {
                total = datos.Count,
                porAsistencia = datos.GroupBy(x => x.Asistencia).Select(g => new { asistencia = g.Key, cantidad = g.Count() }).ToList(),
                lista = datos
            };

            return Json(resumen);
        }

        [HttpGet]
        public async Task<IActionResult> DescargarCSV(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var (desde, hasta) = ObtenerRango(filtro, fechaInicio, fechaFin);

            var datos = await (
                from p in _context.MedicalPsychology
                join pre in _context.PreenrollmentGenerals on p.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where p.AppointmentDatetime >= desde && p.AppointmentDatetime < hasta
                select new
                {
                    Folio = p.Fol,
                    Matricula = pre.Matricula,
                    NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                    Asistencia = p.AttendanceStatus,
                    Fecha = p.AppointmentDatetime
                }
            ).OrderByDescending(x => x.Fecha).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Folio,Matrícula,Nombre Completo,Asistencia,Fecha");
            foreach (var item in datos)
                sb.AppendLine($"{item.Folio},{item.Matricula},\"{item.NombreCompleto}\",{item.Asistencia},{item.Fecha:dd/MM/yyyy HH:mm}");

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Reporte_Psicologia_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> DescargarPDF(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var (desde, hasta) = ObtenerRango(filtro, fechaInicio, fechaFin);

            var datos = await (
                from p in _context.MedicalPsychology
                join pre in _context.PreenrollmentGenerals on p.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where p.AppointmentDatetime >= desde && p.AppointmentDatetime < hasta
                select new
                {
                    Folio = p.Fol,
                    Matricula = pre.Matricula,
                    NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                    Asistencia = p.AttendanceStatus,
                    Fecha = p.AppointmentDatetime
                }
            ).OrderByDescending(x => x.Fecha).ToListAsync();

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, iText.Kernel.Geom.PageSize.LETTER);
            document.SetMargins(30, 30, 30, 30);

            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var colorHeader = new DeviceRgb(98, 9, 0);
            var colorGris = new DeviceRgb(238, 238, 238);

            document.Add(new Paragraph("DEPARTAMENTO DE PSICOLOGÍA").SetFont(fontBold).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph("Reporte de Atenciones Psicológicas").SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER).SetFontColor(new DeviceRgb(100, 100, 100)));
            document.Add(new Paragraph($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").SetFont(fontNormal).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER).SetFontColor(new DeviceRgb(150, 150, 150)).SetMarginBottom(15));
            document.Add(new Paragraph($"Total de registros: {datos.Count}").SetFont(fontBold).SetFontSize(11).SetMarginBottom(5));

            var porAsistencia = datos.GroupBy(x => x.Asistencia).Select(g => $"{g.Key}: {g.Count()}").ToList();
            document.Add(new Paragraph("Desglose por asistencia: " + string.Join("   |   ", porAsistencia)).SetFont(fontNormal).SetFontSize(10).SetMarginBottom(15));

            var tabla = new Table(UnitValue.CreatePercentArray(new float[] { 12, 18, 30, 20, 20 })).UseAllAvailableWidth();
            foreach (var h in new[] { "Folio", "Matrícula", "Alumno", "Asistencia", "Fecha" })
                tabla.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(fontBold).SetFontSize(9).SetFontColor(ColorConstants.WHITE)).SetBackgroundColor(colorHeader).SetPadding(6));

            bool par = false;
            foreach (var item in datos)
            {
                var bg = par ? colorGris : ColorConstants.WHITE;
                tabla.AddCell(new Cell().Add(new Paragraph(item.Folio ?? "").SetFont(fontNormal).SetFontSize(9)).SetBackgroundColor(bg).SetPadding(5));
                tabla.AddCell(new Cell().Add(new Paragraph(item.Matricula ?? "").SetFont(fontNormal).SetFontSize(9)).SetBackgroundColor(bg).SetPadding(5));
                tabla.AddCell(new Cell().Add(new Paragraph(item.NombreCompleto ?? "").SetFont(fontNormal).SetFontSize(9)).SetBackgroundColor(bg).SetPadding(5));
                tabla.AddCell(new Cell().Add(new Paragraph(item.Asistencia ?? "").SetFont(fontNormal).SetFontSize(9)).SetBackgroundColor(bg).SetPadding(5));
                tabla.AddCell(new Cell().Add(new Paragraph(item.Fecha.ToString("dd/MM/yyyy")).SetFont(fontNormal).SetFontSize(9)).SetBackgroundColor(bg).SetPadding(5));
                par = !par;
            }

            document.Add(tabla);
            document.Close();
            return File(ms.ToArray(), "application/pdf", $"Reporte_Psicologia_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private (DateTime desde, DateTime hasta) ObtenerRango(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var hoy = DateTime.Today;
            DateTime desde;
            DateTime hasta = hoy.AddDays(1);
            switch (filtro)
            {
                case "dia": desde = hoy; break;
                case "semana": desde = hoy.AddDays(-7); break;
                case "mes": desde = hoy.AddDays(-30); break;
                case "personalizado":
                    desde = fechaInicio ?? hoy;
                    hasta = (fechaFin ?? hoy).AddDays(1);
                    break;
                default: desde = hoy; break;
            }
            return (desde, hasta);
        }

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

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
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
            model.StaffId = null;

            _context.MedicalPsychology.Add(model);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Registro psicológico creado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cita = await _context.MedicalPsychology.FindAsync(id);
            if (cita == null) return NotFound();
            return View(cita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, medical_pychology model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Registro psicológico actualizado correctamente.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

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
                    AppointmentDatetime = p.AppointmentDatetime,
                    AttendanceStatus = p.AttendanceStatus,
                    PreenrollmentId = p.PreenrollmentId,
                    MatriculaTemp = pre.Matricula,
                    NombreCompletoTemp = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal
                }
            ).FirstOrDefaultAsync();

            if (data == null) return NotFound();
            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cita = await _context.MedicalPsychology.FindAsync(id);
            if (cita != null)
            {
                _context.MedicalPsychology.Remove(cita);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Registro psicológico eliminado correctamente.";
                TempData["Tipo"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> BuscarPorMatricula(string matricula)
        {
            var data = await (
                from pre in _context.PreenrollmentGenerals
                join per in _context.Persons on pre.UserId equals per.PersonId
                where pre.Matricula == matricula
                select new
                {
                    preId = pre.IdData,
                    nombre = per.FirstName,
                    paterno = per.LastNamePaternal,
                    materno = per.LastNameMaternal
                }
            ).FirstOrDefaultAsync();

            if (data == null) return Json(null);
            return Json(data);
        }
    }
}