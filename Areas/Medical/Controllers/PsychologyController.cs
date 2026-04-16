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
using SchoolManager.Areas.Medical.Filters;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.ViewModels;
using System.Security.Claims;
using System.Text;

#nullable disable

namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Psychologist,Head of Psychology,Coordinator,Master")]
    
    [ServiceFilter(typeof(MedicalPermissionFilter))]
    public class PsychologyController : Controller
    {
        private readonly AppDbContext _context;

        public PsychologyController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<medical_permissions?> ObtenerPermisos()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("PersonId")
                        ?? User.FindFirst("UserId");

            if (claim == null) return null;

            var userId = int.Parse(claim.Value!);

            var staff = await _context.MedicalStaff
                .FirstOrDefaultAsync(s => s.PersonId == userId);

            if (staff == null) return null;

            return await _context.MedicalPermissions
                .FirstOrDefaultAsync(p => p.StaffId == staff.Id);
        }

        public async Task<IActionResult> Index()
        {
            var permisos = await ObtenerPermisos();
            ViewBag.Permisos = permisos;

            if (User.IsInRole("Psychologist"))
            {
                if (permisos != null && !permisos.Ver)
                    return View("AccesoDenegado");
            }

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

        [Authorize(Roles = "Psychologist,Head of Psychology,Master")]
        public async Task<IActionResult> Create()
        {
            var permisos = await ObtenerPermisos();

            if (User.IsInRole("Psychologist"))
            {
                if (permisos != null && !permisos.Agregar)
                    return View("AccesoDenegado");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Psychologist,Head of Psychology,Master")]
        public async Task<IActionResult> Create(medical_pychology model)
        {
            var permisos = await ObtenerPermisos();

            if (User.IsInRole("Psychologist"))
            {
                if (permisos != null && !permisos.Agregar)
                    return View("AccesoDenegado");
            }

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

        public async Task<IActionResult> Details(int id)
        {
            var permisos = await ObtenerPermisos();
            ViewBag.Permisos = permisos;

            if (User.IsInRole("Psychologist"))
            {
                if (permisos != null && !permisos.Ver)
                    return View("AccesoDenegado");
            }

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

        [Authorize(Roles = "Psychologist,Head of Psychology,Master")]
        public async Task<IActionResult> Edit(int id)
        {
            var permisos = await ObtenerPermisos();

            if (User.IsInRole("Coordinator"))
                return View("AccesoDenegado");

            if (User.IsInRole("Psychologist"))
            {
                if (permisos != null && !permisos.Modificar)
                    return View("AccesoDenegado");
            }

            var cita = await _context.MedicalPsychology.FindAsync(id);
            if (cita == null) return NotFound();
            return View(cita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Psychologist,Head of Psychology,Master")]
        public async Task<IActionResult> Edit(int id, medical_pychology model)
        {
            var permisos = await ObtenerPermisos();

            if (User.IsInRole("Coordinator"))
                return View("AccesoDenegado");

            if (User.IsInRole("Psychologist"))
            {
                if (permisos != null && !permisos.Modificar)
                    return View("AccesoDenegado");
            }

            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var bd = await _context.MedicalPsychology
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (bd == null) return NotFound();

                model.CreatedAt = bd.CreatedAt;

                _context.Update(model);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Registro actualizado correctamente.";
                TempData["Tipo"] = "warning";

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [Authorize(Roles = "Psychologist,Head of Psychology,Master")]
        public async Task<IActionResult> Delete(int id)
        {
            var permisos = await ObtenerPermisos();

            if (User.IsInRole("Coordinator"))
                return View("AccesoDenegado");

            if (User.IsInRole("Psychologist"))
            {
                if (permisos != null && !permisos.Borrar)
                    return View("AccesoDenegado");
            }

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
                    MatriculaTemp = pre.Matricula,
                    NombreCompletoTemp = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal
                }
            ).FirstOrDefaultAsync();

            if (data == null) return NotFound();
            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Psychologist,Head of Psychology,Master")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var permisos = await ObtenerPermisos();

            if (User.IsInRole("Coordinator"))
                return View("AccesoDenegado");

            if (User.IsInRole("Psychologist"))
            {
                if (permisos != null && !permisos.Borrar)
                    return View("AccesoDenegado");
            }

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

        private (DateTime desde, DateTime hasta) ObtenerRango(string filtro, DateTime? fechaInicio, DateTime? fechaFin) { return (DateTime.Now, DateTime.Now); }

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

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosReporte(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = from p in _context.MedicalPsychology
                        join pre in _context.PreenrollmentGenerals on p.PreenrollmentId equals pre.IdData
                        join per in _context.Persons on pre.UserId equals per.PersonId
                        select new
                        {
                            p.Fol,
                            pre.Matricula,
                            NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                            p.AttendanceStatus,
                            p.AppointmentDatetime
                        };

            DateTime hoy = DateTime.Today;

            if (filtro == "dia")
                query = query.Where(x => x.AppointmentDatetime.Date == hoy);

            else if (filtro == "semana")
                query = query.Where(x => x.AppointmentDatetime >= hoy.AddDays(-7));

            else if (filtro == "mes")
                query = query.Where(x => x.AppointmentDatetime >= hoy.AddMonths(-1));

            else if (filtro == "personalizado" && fechaInicio.HasValue && fechaFin.HasValue)
                query = query.Where(x =>
                    x.AppointmentDatetime.Date >= fechaInicio.Value.Date &&
                    x.AppointmentDatetime.Date <= fechaFin.Value.Date);

            var lista = await query.OrderByDescending(x => x.AppointmentDatetime).ToListAsync();

            var total = lista.Count;

            var porAsistencia = lista
                .GroupBy(x => x.AttendanceStatus)
                .Select(g => new
                {
                    asistencia = g.Key,
                    cantidad = g.Count()
                })
                .ToList();

            return Json(new
            {
                total,
                porAsistencia,
                lista = lista.Select(x => new
                {
                    folio = x.Fol,
                    matricula = x.Matricula,
                    nombreCompleto = x.NombreCompleto,
                    asistencia = x.AttendanceStatus,
                    fecha = x.AppointmentDatetime
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> DescargarCSV(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var result = await ObtenerDatosReporte(filtro, fechaInicio, fechaFin) as JsonResult;

            if (result?.Value == null)
                return BadRequest();

            dynamic data = result.Value;
            var lista = data.lista;
            int total = data.total;
            var porAsistencia = data.porAsistencia;

            var sb = new StringBuilder();

            // Encabezado tipo reporte
            sb.AppendLine("DEPARTAMENTO DE PSICOLOGÍA");
            sb.AppendLine("Reporte de Atenciones Psicológicas");
            sb.AppendLine($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine("");

            // Resumen
            sb.AppendLine($"Total de atenciones: {total}");

            string desglose = "Desglose por asistencia: ";
            foreach (var item in porAsistencia)
            {
                desglose += $"{item.asistencia}: {item.cantidad} | ";
            }
            sb.AppendLine(desglose.TrimEnd(' ', '|'));

            sb.AppendLine("");

            // Encabezados de tabla
            sb.AppendLine("Folio;Matrícula;Alumno;Asistencia;Fecha");

            // Datos
            foreach (var item in lista)
            {
                string alumno = $"\"{item.nombreCompleto}\""; 

                sb.AppendLine($"{item.folio};{item.matricula};{alumno};{item.asistencia};{Convert.ToDateTime(item.fecha):dd/MM/yyyy}");
            }

            return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray(), "text/csv", "reporte_psicologia.csv");
        }

        [HttpGet]
        public async Task<IActionResult> DescargarPDF(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var result = await ObtenerDatosReporte(filtro, fechaInicio, fechaFin) as JsonResult;

            if (result?.Value == null)
                return BadRequest();

            dynamic data = result.Value;
            var lista = data.lista;
            int total = data.total;
            var porAsistencia = data.porAsistencia;

            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var doc = new Document(pdf);

                var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // TÍTULO
                doc.Add(new Paragraph("DEPARTAMENTO DE PSICOLOGÍA")
                    .SetFont(bold)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.CENTER));

                doc.Add(new Paragraph("Reporte de Atenciones Psicológicas")
                    .SetFont(normal)
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.CENTER));

                doc.Add(new Paragraph("Generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER));

                doc.Add(new Paragraph("\n"));

                // TOTAL
                doc.Add(new Paragraph($"Total de atenciones: {total}")
                    .SetFont(bold)
                    .SetFontSize(11));

                // DESGLOSE
                string desglose = "Desglose por asistencia: ";
                foreach (var item in porAsistencia)
                {
                    desglose += $"{item.asistencia}: {item.cantidad}   |   ";
                }

                doc.Add(new Paragraph(desglose.TrimEnd(' ', '|'))
                    .SetFont(normal)
                    .SetFontSize(10));

                doc.Add(new Paragraph("\n"));

                // TABLA
                var table = new Table(new float[] { 2, 3, 5, 3, 3 })
                    .UseAllAvailableWidth();

                Color rojo = new DeviceRgb(98, 9, 0);

                void HeaderCell(string text)
                {
                    table.AddHeaderCell(
                        new Cell()
                        .Add(new Paragraph(text).SetFont(bold).SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(rojo)
                        .SetTextAlignment(TextAlignment.CENTER)
                    );
                }

                HeaderCell("Folio");
                HeaderCell("Matrícula");
                HeaderCell("Alumno");
                HeaderCell("Asistencia");
                HeaderCell("Fecha");

                foreach (var item in lista)
                {
                    table.AddCell(new Paragraph(item.folio).SetFont(normal));
                    table.AddCell(new Paragraph(item.matricula).SetFont(normal));
                    table.AddCell(new Paragraph(item.nombreCompleto).SetFont(normal));
                    table.AddCell(new Paragraph(item.asistencia).SetFont(normal));
                    table.AddCell(new Paragraph(Convert.ToDateTime(item.fecha).ToString("dd/MM/yyyy")).SetFont(normal));
                }

                doc.Add(table);

                doc.Close();

                return File(ms.ToArray(), "application/pdf", "reporte_psicologia.pdf");
            }
        }
    }
}