using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Medical.Filters;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.ViewModels;
using System.Security.Claims;
using System.Text;

namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Nurse,Head Nurse,Coordinator,Master")]
    
    [ServiceFilter(typeof(MedicalPermissionFilter))]
    public class LogbookController : Controller
    {
        private readonly AppDbContext _context;

        public LogbookController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<medical_permissions?> ObtenerPermisos()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("PersonId")
                        ?? User.FindFirst("UserId");

            if (claim == null) return null;

            var userId = int.Parse(claim.Value);

            var staff = await _context.MedicalStaff
                .FirstOrDefaultAsync(s => s.PersonId == userId);

            if (staff == null) return null;

            return await _context.MedicalPermissions
                .FirstOrDefaultAsync(p => p.StaffId == staff.Id);
        }

        public async Task<IActionResult> Index(string matricula)
        {
            var permisos = await ObtenerPermisos();
            ViewBag.Permisos = permisos;

            if (User.IsInRole("Nurse"))
            {
                if (permisos == null || !permisos.Agregar)
                    return View("AccesoDenegado");
            }

            ViewData["CurrentFilter"] = matricula;
            string searchString = (matricula ?? string.Empty).Trim();

            var query = from b in _context.MedicalLogbooks
                        join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                        join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                        join u in _context.Users on pre.UserId equals u.UserId
                        join per in _context.Persons on u.PersonId equals per.PersonId
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

        public IActionResult Reporte()
        {
            return View();
        }

        [Authorize(Roles = "Nurse,Head Nurse,Master,Coordinator")]
        public async Task<IActionResult> Create()
        {
            var permisos = await ObtenerPermisos();

            if (User.IsInRole("Nurse"))
            {
                if (permisos == null || !permisos.Agregar)
                    return View("AccesoDenegado");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Nurse,Head Nurse,Master,Coordinator")]
        public async Task<IActionResult> Create(medical_logbook bitacora)
        {
            var permisos = await ObtenerPermisos();

            if (User.IsInRole("Nurse"))
            {
                if (permisos == null || !permisos.Agregar)
                    return View("AccesoDenegado");
            }

            if (bitacora.IdAlumno == 0)
            {
                ModelState.AddModelError("", "Debe buscar un alumno válido");
                return View(bitacora);
            }

            var ultimoFol = await _context.MedicalLogbooks
                .OrderByDescending(x => x.Id)
                .Select(x => x.Folio)
                .FirstOrDefaultAsync();

            int nuevoNumero = 1;
            if (!string.IsNullOrEmpty(ultimoFol))
            {
                int.TryParse(ultimoFol, out nuevoNumero);
                nuevoNumero++;
            }

            bitacora.Folio = nuevoNumero.ToString("D6");
            bitacora.FechaHora = DateTime.Now;
            bitacora.CreatedAt = DateTime.Now;

            _context.MedicalLogbooks.Add(bitacora);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var permisos = await ObtenerPermisos();
            ViewBag.Permisos = permisos;

            if (User.IsInRole("Nurse"))
            {
                if (permisos == null || !permisos.Agregar)
                    return View("AccesoDenegado");
            }

            var bitacora = await (
                from b in _context.MedicalLogbooks
                join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                join u in _context.Users on pre.UserId equals u.UserId
                join per in _context.Persons on u.PersonId equals per.PersonId
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

        [Authorize(Roles = "Nurse,Master,Head Nurse,Coordinator")]
        public async Task<IActionResult> Edit(int id)
        {
            var permisos = await ObtenerPermisos();

            if (User.IsInRole("Coordinator"))
                return View("AccesoDenegado");

            if (User.IsInRole("Nurse"))
            {
                if (permisos == null || !permisos.Agregar)
                    return View("AccesoDenegado");
            }

            var bitacora = await _context.MedicalLogbooks.FindAsync(id);
            if (bitacora == null) return NotFound();

            return View(bitacora);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Nurse,Master,Head Nurse,Coordinator")]
        public async Task<IActionResult> Edit(int id, medical_logbook bitacora)
        {
            var permisos = await ObtenerPermisos();

            if (User.IsInRole("Coordinator"))
                return View("AccesoDenegado");

            if (User.IsInRole("Nurse"))
            {
                if (permisos == null || !permisos.Agregar)
                    return View("AccesoDenegado");
            }

            if (id != bitacora.Id) return NotFound();

            var bd = await _context.MedicalLogbooks.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            bitacora.CreatedAt = bd.CreatedAt;

            _context.Update(bitacora);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Nurse,Head Nurse,Master")]
        public async Task<IActionResult> Delete(int id)
        {
            var permisos = await ObtenerPermisos();

            if (User.IsInRole("Nurse"))
            {
                if (permisos == null || !permisos.Borrar)
                    return View("AccesoDenegado");
            }

            var bitacora = await (
                from b in _context.MedicalLogbooks
                join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                join u in _context.Users on pre.UserId equals u.UserId
                join per in _context.Persons on u.PersonId equals per.PersonId
                where b.Id == id
                select new medical_logbook
                {
                    Id = b.Id,
                    Folio = b.Folio,
                    FechaHora = b.FechaHora,
                    Estado = b.Estado,
                    MotivoConsulta = b.MotivoConsulta,
                    Observaciones = b.Observaciones,
                    Tratamiento = b.Tratamiento,
                    SignosVitales = b.SignosVitales,
                    MatriculaTemp = pre.Matricula,
                    NombreCompletoTemp = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal
                }
            ).FirstOrDefaultAsync();

            if (bitacora == null) return NotFound();
            return View(bitacora);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Nurse,Head Nurse,Master")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var permisos = await ObtenerPermisos();

            // Nurse necesita permiso
            if (User.IsInRole("Nurse"))
            {
                if (permisos == null || !permisos.Borrar)
                    return View("AccesoDenegado");
            }

            var bitacora = await _context.MedicalLogbooks.FindAsync(id);
            if (bitacora == null) return NotFound();

            _context.MedicalLogbooks.Remove(bitacora);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> BuscarPorMatricula(string matricula)
        {
            if (string.IsNullOrEmpty(matricula))
                return Json(null);

            var data = await (
                from alumno in _context.MedicalStudents
                join pre in _context.PreenrollmentGenerals on alumno.PreenrollmentId equals pre.IdData
                join u in _context.Users on pre.UserId equals u.UserId
                join per in _context.Persons on u.PersonId equals per.PersonId
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

        [HttpGet]
public async Task<IActionResult> ObtenerDatosReporte(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
{
    var query = from b in _context.MedicalLogbooks
                join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                join u in _context.Users on pre.UserId equals u.UserId
                join per in _context.Persons on u.PersonId equals per.PersonId
                select new
                {
                    b.Folio,
                    pre.Matricula,
                    NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                    b.MotivoConsulta,
                    b.Estado,
                    b.FechaHora
                };

    DateTime hoy = DateTime.Today;

    if (filtro == "dia")
        query = query.Where(x => x.FechaHora.Date == hoy);

    else if (filtro == "semana")
        query = query.Where(x => x.FechaHora >= hoy.AddDays(-7));

    else if (filtro == "mes")
        query = query.Where(x => x.FechaHora >= hoy.AddMonths(-1));

    else if (filtro == "personalizado" && fechaInicio.HasValue && fechaFin.HasValue)
        query = query.Where(x => x.FechaHora.Date >= fechaInicio.Value.Date && x.FechaHora.Date <= fechaFin.Value.Date);

    var lista = await query.OrderByDescending(x => x.FechaHora).ToListAsync();

    var total = lista.Count;

    var porEstado = lista
        .GroupBy(x => x.Estado)
        .Select(g => new { estado = g.Key, cantidad = g.Count() })
        .ToList();

    return Json(new
    {
        total,
        porEstado,
        lista = lista.Select(x => new
        {
            folio = x.Folio,
            matricula = x.Matricula,
            nombreCompleto = x.NombreCompleto,
            motivo = x.MotivoConsulta,
            estado = x.Estado,
            fecha = x.FechaHora
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
            var porEstado = data.porEstado;

            var sb = new StringBuilder();

            // ENCABEZADO
            sb.AppendLine("DEPARTAMENTO DE ENFERMERÍA");
            sb.AppendLine("Reporte de Atenciones Clínicas");
            sb.AppendLine($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine("");

            // RESUMEN
            sb.AppendLine($"Total de atenciones: {total}");

            string desglose = "Desglose por estado: ";
            foreach (var item in porEstado)
            {
                desglose += $"{item.estado}: {item.cantidad} | ";
            }

            sb.AppendLine(desglose.TrimEnd(' ', '|'));
            sb.AppendLine("");

            // ENCABEZADOS TABLA
            sb.AppendLine("Folio;Matrícula;Alumno;Motivo;Estado;Fecha");

            // DATOS
            foreach (var item in lista)
            {
                string alumno = $"\"{item.nombreCompleto}\"";

                sb.AppendLine($"{item.folio};{item.matricula};{alumno};{item.motivo};{item.estado};{Convert.ToDateTime(item.fecha):dd/MM/yyyy}");
            }

            return File(
                Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray(),
                "text/csv",
                "reporte_enfermeria.csv"
            );
        }

        [HttpGet]
        public async Task<IActionResult> DescargarPDF(string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var json = await ObtenerDatosReporte(filtro, fechaInicio, fechaFin) as JsonResult;

            if (json == null || json.Value == null)
                return BadRequest();

            dynamic result = json.Value;

            if (result.lista == null)
                return BadRequest();

            var lista = result.lista;
            int total = result.total;
            var porEstado = result.porEstado;

            using (var ms = new MemoryStream())
            {
                var writer = new iText.Kernel.Pdf.PdfWriter(ms);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var doc = new iText.Layout.Document(pdf);

                var bold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                var normal = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

                // TÍTULO
                doc.Add(new iText.Layout.Element.Paragraph("DEPARTAMENTO DE ENFERMERÍA")
                    .SetFont(bold)
                    .SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                doc.Add(new iText.Layout.Element.Paragraph("Reporte de Atenciones Clínicas")
                    .SetFont(normal)
                    .SetFontSize(11)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                doc.Add(new iText.Layout.Element.Paragraph("Generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                    .SetFontSize(9)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                doc.Add(new iText.Layout.Element.Paragraph("\n"));

                // TOTAL
                doc.Add(new iText.Layout.Element.Paragraph($"Total de atenciones: {total}")
                    .SetFont(bold)
                    .SetFontSize(11));

                // DESGLOSE
                string desglose = "Desglose por estado: ";
                foreach (var item in porEstado)
                {
                    desglose += $"{item.estado}: {item.cantidad}   |   ";
                }

                doc.Add(new iText.Layout.Element.Paragraph(desglose.TrimEnd(' ', '|'))
                    .SetFont(normal)
                    .SetFontSize(10));

                doc.Add(new iText.Layout.Element.Paragraph("\n"));

                // TABLA
                var table = new iText.Layout.Element.Table(new float[] { 2, 3, 5, 4, 3, 3 })
                    .UseAllAvailableWidth();

                var rojo = new iText.Kernel.Colors.DeviceRgb(98, 9, 0);

                void HeaderCell(string text)
                {
                    table.AddHeaderCell(
                        new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(text)
                        .SetFont(bold)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                        .SetBackgroundColor(rojo)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    );
                }

                HeaderCell("Folio");
                HeaderCell("Matrícula");
                HeaderCell("Alumno");
                HeaderCell("Motivo");
                HeaderCell("Estado");
                HeaderCell("Fecha");

                foreach (var item in lista)
                {
                    table.AddCell(new iText.Layout.Element.Paragraph(item.folio).SetFont(normal));
                    table.AddCell(new iText.Layout.Element.Paragraph(item.matricula).SetFont(normal));
                    table.AddCell(new iText.Layout.Element.Paragraph(item.nombreCompleto).SetFont(normal));
                    table.AddCell(new iText.Layout.Element.Paragraph(item.motivo).SetFont(normal));
                    table.AddCell(new iText.Layout.Element.Paragraph(item.estado).SetFont(normal));
                    table.AddCell(new iText.Layout.Element.Paragraph(Convert.ToDateTime(item.fecha).ToString("dd/MM/yyyy")).SetFont(normal));
                }

                doc.Add(table);

                doc.Close();

                return File(ms.ToArray(), "application/pdf", "reporte_enfermeria.pdf");
            }
        }
    }
}