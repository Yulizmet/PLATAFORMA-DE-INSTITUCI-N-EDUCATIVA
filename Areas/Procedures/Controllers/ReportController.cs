using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Helpers;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConverter _converter;

        public ReportController(AppDbContext context, IConverter converter)
        {
            _context = context;
            _converter = converter;
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(string entity)
        {
            var (data, columns) = await GetEntityMetadata(entity);
            if (data == null || !data.Any()) return NotFound();

            var builder = new System.Text.StringBuilder();

            var headers = new List<string> { "#" };
            headers.AddRange(columns.Select(c => (string)c.DisplayName));
            builder.AppendLine(string.Join(";", headers));

            int contador = 1;
            foreach (var item in data)
            {
                var row = new List<string> { contador.ToString() };

                foreach (var col in columns)
                {
                    var prop = item.GetType().GetProperty(col.PropName);
                    var value = prop?.GetValue(item, null);
                    string formattedValue = "";

                    if (value is DateTime dt)
                        formattedValue = dt.ToString("dd/MM/yyyy hh:mm tt");
                    else
                        formattedValue = value?.ToString() ?? "";

                    formattedValue = formattedValue.Replace(";", " ").Replace("\r", "").Replace("\n", " ");
                    row.Add(formattedValue);
                }

                builder.AppendLine(string.Join(";", row));
                contador++;
            }

            var csvBytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();

            return File(csvBytes, "text/csv", $"Reporte_{entity}_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string entity)
        {
            var (data, columns) = await GetEntityMetadata(entity);
            if (data == null || !data.Any()) return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "ReportExcel.xlsx");

            if (!System.IO.File.Exists(filePath)) return BadRequest("Plantilla no encontrada.");

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);

                worksheet.Rows(6, 1000).Clear(XLClearOptions.All);

                worksheet.Cell(2, 2).Value = $"REPORTE OFICIAL DE {entity.ToUpper()}";

                int colHeader = 1;
                foreach (var col in columns)
                {
                    var cell = worksheet.Cell(5, colHeader);
                    cell.Value = col.DisplayName;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#8C1B1B");
                    cell.Style.Font.FontColor = XLColor.White;
                    colHeader++;
                }

                int currentRow = 6;
                foreach (var item in data)
                {
                    int currentCol = 1;
                    foreach (var col in columns)
                    {
                        var prop = item.GetType().GetProperty(col.PropName);
                        var value = prop?.GetValue(item, null);

                        var cell = worksheet.Cell(currentRow, currentCol);

                        if (value is DateTime dt)
                            cell.Value = dt.ToString("dd/MM/yyyy hh:mm tt");
                        else
                            cell.Value = value?.ToString() ?? "";

                        cell.Style.Border.BottomBorder = XLBorderStyleValues.None;

                        currentCol++;
                    }
                    currentRow++;
                }

                worksheet.Columns(1, columns.Count).AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Reporte_{entity}_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(string entity)
        {
            var (data, columns) = await GetEntityMetadata(entity);
            if (data == null || !data.Any()) return NotFound();

            ViewData["Entity"] = entity;
            ViewData["Columns"] = columns;

            string htmlContent = await this.RenderViewAsync("_ReportPDF", data, true);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.Letter,
                Margins = new MarginSettings { Top = 0, Bottom = 0, Left = 0, Right = 0 }
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" },
                HeaderSettings = { HtmUrl = null, Center = null, Left = null, Right = null, Line = false, Spacing = 0 },
                FooterSettings = { HtmUrl = null, Center = null, Left = null, Right = null, Line = false, Spacing = 0 }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);
            return File(file, "application/pdf", $"Reporte_{entity}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private async Task<(List<object>? data, List<dynamic> columns)> GetEntityMetadata(string entity)
        {
            List<object>? data = entity.ToLower() switch
            {
                "areas" => (await _context.ProcedureAreas.ToListAsync()).Cast<object>().ToList(),
                "tramites" => (await _context.ProcedureTypes.Include(p => p.ProcedureArea).ToListAsync()).Cast<object>().ToList(),
                "documentos" => (await _context.ProcedureTypeDocuments.ToListAsync()).Cast<object>().ToList(),
                "estados" => (await _context.ProcedureStatus.ToListAsync()).Cast<object>().ToList(),
                _ => null
            };

            if (data == null || !data.Any()) return (null, new List<dynamic>());

            var columns = data.First().GetType().GetProperties()
                .Where(p => p.GetCustomAttribute<DisplayAttribute>() != null)
                .Select(p => new
                {
                    PropName = p.Name,
                    DisplayName = p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name
                } as dynamic).ToList();

            return (data, columns);
        }
    }
}