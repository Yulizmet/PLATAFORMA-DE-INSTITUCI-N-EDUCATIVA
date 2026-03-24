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
using System.Text;
#nullable disable

namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Nurse,Psychologist,Head Nurse,Head of Psychology,Coordinator,Master")]
    public class LogbookController : Controller
    {
        private readonly AppDbContext _context;

        public LogbookController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string matricula)
        {
            ViewData["CurrentFilter"] = matricula;
            string searchString = (matricula ?? string.Empty).Trim();

            var query = from b in _context.MedicalLogbooks
                        join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                        join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                        join per in _context.Persons on pre.UserId equals per.PersonId
                        select new LogBookListVM
                        {
                            Id = b.Id,
                            Folio = b.Folio,
                            Matricula = pre.Matricula,
                            NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                            Motivo = b.MotivoConsulta,
                            Estado = b.Estado,
                            Fecha = b.FechaHora
                        };

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(s => s.Matricula.Contains(searchString));

            var lista = await query.OrderByDescending(x => x.Id).ToListAsync();
            return View(lista);
        }

        public IActionResult Reporte() => View();

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosReporte(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var (desde, hasta) = ObtenerRango(filtro, fechaInicio, fechaFin);

            var datos = await (
                from b in _context.MedicalLogbooks
                join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where b.FechaHora >= desde && b.FechaHora < hasta
                select new LogBookListVM
                {
                    Id = b.Id,
                    Folio = b.Folio,
                    Matricula = pre.Matricula,
                    NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                    Motivo = b.MotivoConsulta,
                    Estado = b.Estado,
                    Fecha = b.FechaHora
                }
            ).OrderByDescending(x => x.Fecha).ToListAsync();

            var resumen = new
            {
                total = datos.Count,
                porEstado = datos.GroupBy(x => x.Estado).Select(g => new { estado = g.Key, cantidad = g.Count() }).ToList(),
                lista = datos
            };
            return Json(resumen);
        }

        [HttpGet]
        public async Task<IActionResult> DescargarCSV(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var (desde, hasta) = ObtenerRango(filtro, fechaInicio, fechaFin);

            var datos = await (
                from b in _context.MedicalLogbooks
                join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where b.FechaHora >= desde && b.FechaHora < hasta
                select new
                {
                    b.Folio,
                    Matricula = pre.Matricula,
                    NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                    Motivo = b.MotivoConsulta,
                    Estado = b.Estado,
                    Fecha = b.FechaHora
                }
            ).OrderByDescending(x => x.Fecha).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Folio,Matrícula,Nombre Completo,Motivo,Estado,Fecha");
            foreach (var item in datos)
                sb.AppendLine($"{item.Folio},{item.Matricula},\"{item.NombreCompleto}\",\"{item.Motivo}\",{item.Estado},{item.Fecha:dd/MM/yyyy HH:mm}");

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Reporte_Bitacoras_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> DescargarPDF(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var (desde, hasta) = ObtenerRango(filtro, fechaInicio, fechaFin);

            var datos = await (
                from b in _context.MedicalLogbooks
                join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where b.FechaHora >= desde && b.FechaHora < hasta
                select new
                {
                    b.Folio,
                    Matricula = pre.Matricula,
                    NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                    Motivo = b.MotivoConsulta,
                    Estado = b.Estado,
                    Fecha = b.FechaHora
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
            var colorHeaderText = ColorConstants.WHITE;
            var colorGris = new DeviceRgb(238, 238, 238);

            document.Add(new Paragraph("DEPARTAMENTO MÉDICO").SetFont(fontBold).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph("Reporte de Atenciones Clínicas").SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER).SetFontColor(new DeviceRgb(100, 100, 100)));
            document.Add(new Paragraph($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").SetFont(fontNormal).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER).SetFontColor(new DeviceRgb(150, 150, 150)).SetMarginBottom(15));
            document.Add(new Paragraph($"Total de atenciones: {datos.Count}").SetFont(fontBold).SetFontSize(11).SetMarginBottom(5));

            var porEstado = datos.GroupBy(x => x.Estado).Select(g => $"{g.Key}: {g.Count()}").ToList();
            document.Add(new Paragraph("Desglose por estado: " + string.Join("   |   ", porEstado)).SetFont(fontNormal).SetFontSize(10).SetMarginBottom(15));

            var tabla = new Table(UnitValue.CreatePercentArray(new float[] { 10, 15, 25, 25, 15, 15 })).UseAllAvailableWidth();
            foreach (var h in new[] { "Folio", "Matrícula", "Alumno", "Motivo", "Estado", "Fecha" })
                tabla.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(fontBold).SetFontSize(9).SetFontColor(colorHeaderText)).SetBackgroundColor(colorHeader).SetPadding(6));

            bool par = false;
            foreach (var item in datos)
            {
                var bg = par ? colorGris : ColorConstants.WHITE;
                tabla.AddCell(new Cell().Add(new Paragraph(item.Folio ?? "").SetFont(fontNormal).SetFontSize(9)).SetBackgroundColor(bg).SetPadding(5));
                tabla.AddCell(new Cell().Add(new Paragraph(item.Matricula ?? "").SetFont(fontNormal).SetFontSize(9)).SetBackgroundColor(bg).SetPadding(5));
                tabla.AddCell(new Cell().Add(new Paragraph(item.NombreCompleto ?? "").SetFont(fontNormal).SetFontSize(9)).SetBackgroundColor(bg).SetPadding(5));
                tabla.AddCell(new Cell().Add(new Paragraph(item.Motivo ?? "").SetFont(fontNormal).SetFontSize(9)).SetBackgroundColor(bg).SetPadding(5));
                tabla.AddCell(new Cell().Add(new Paragraph(item.Estado ?? "").SetFont(fontNormal).SetFontSize(9)).SetBackgroundColor(bg).SetPadding(5));
                tabla.AddCell(new Cell().Add(new Paragraph(item.Fecha.ToString("dd/MM/yyyy")).SetFont(fontNormal).SetFontSize(9)).SetBackgroundColor(bg).SetPadding(5));
                par = !par;
            }

            document.Add(tabla);
            document.Close();
            return File(ms.ToArray(), "application/pdf", $"Reporte_Bitacoras_{DateTime.Now:yyyyMMdd}.pdf");
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
            var bitacora = await (
                from b in _context.MedicalLogbooks
                join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where b.Id == id
                select new medical_logbook
                {
                    Id = b.Id,
                    Folio = b.Folio,
                    FechaHora = b.FechaHora,
                    Estado = b.Estado,
                    MotivoConsulta = b.MotivoConsulta,
                    SignosVitales = b.SignosVitales,
                    Observaciones = b.Observaciones,
                    Tratamiento = b.Tratamiento,
                    MatriculaTemp = pre.Matricula,
                    NombreCompletoTemp = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal
                }
            ).FirstOrDefaultAsync();

            if (bitacora == null) return NotFound();
            return View(bitacora);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var bitacora = await _context.MedicalLogbooks.FindAsync(id);
            if (bitacora == null) return NotFound();
            return View(bitacora);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, medical_logbook bitacora)
        {
            if (id != bitacora.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(bitacora);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Bitácora actualizada correctamente.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }
            return View(bitacora);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(medical_logbook bitacora)
        {
            if (bitacora.IdAlumno == 0)
            {
                ModelState.AddModelError("", "Debe buscar un alumno válido con la matrícula");
                return View(bitacora);
            }

            var ultimoFol = await _context.MedicalLogbooks.OrderByDescending(x => x.Id).Select(x => x.Folio).FirstOrDefaultAsync();
            int nuevoNumero = 1;
            if (!string.IsNullOrEmpty(ultimoFol)) { int.TryParse(ultimoFol, out nuevoNumero); nuevoNumero++; }

            bitacora.Folio = nuevoNumero.ToString("D6");
            bitacora.FechaHora = DateTime.Now;
            bitacora.CreatedAt = DateTime.Now;
            bitacora.IdPersonal = null;

            _context.MedicalLogbooks.Add(bitacora);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Bitácora creada correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var bitacora = await (
                from b in _context.MedicalLogbooks
                join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where b.Id == id
                select new medical_logbook
                {
                    Id = b.Id,
                    Folio = b.Folio,
                    FechaHora = b.FechaHora,
                    Estado = b.Estado,
                    MotivoConsulta = b.MotivoConsulta,
                    MatriculaTemp = pre.Matricula,
                    NombreCompletoTemp = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal
                }
            ).FirstOrDefaultAsync();

            if (bitacora == null) return NotFound();
            return View(bitacora);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bitacora = await _context.MedicalLogbooks.FindAsync(id);
            if (bitacora == null) return NotFound();
            _context.MedicalLogbooks.Remove(bitacora);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Bitácora eliminada correctamente.";
            TempData["Tipo"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> BuscarPorMatricula(string matricula)
        {
            if (string.IsNullOrEmpty(matricula)) return Json(null);

            var data = await (
                from alumno in _context.MedicalStudents
                join pre in _context.PreenrollmentGenerals on alumno.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where pre.Matricula == matricula
                select new
                {
                    alumnoId = alumno.Id,
                    nombre = per.FirstName,
                    paterno = per.LastNamePaternal,
                    materno = per.LastNameMaternal,
                    sangre = pre.BloodType,
                    peso = alumno.Peso,
                    alergias = alumno.Alergias,
                    condiciones = alumno.CondicionesCronicas
                }
            ).FirstOrDefaultAsync();

            return Json(data);
        }
    }
}