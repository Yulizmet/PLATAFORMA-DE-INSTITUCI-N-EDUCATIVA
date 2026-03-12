// Areas/Grades/Controllers/BulkGradeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using SchoolManager.Areas.Grades.ViewModels.BulkGrade;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    public class BulkGradeController : Controller
    {
        private readonly AppDbContext _context;

        public BulkGradeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Grades/BulkGrade/SelectClass
        public async Task<IActionResult> SelectClass()
        {
            var teacherId = GetCurrentTeacherId();

            var classes = await _context.grades_TeacherSubjectGroups
                .Include(tsg => tsg.TeacherSubject)
                    .ThenInclude(ts => ts.Subject)
                        .ThenInclude(s => s.GradeLevel)
                .Include(tsg => tsg.Group)
                .Where(tsg => tsg.TeacherSubject.TeacherId == teacherId)
                .Select(tsg => new
                {
                    tsg.TeacherSubjectGroupId,
                    tsg.GroupId,
                    GroupName = tsg.Group.Name,
                    tsg.TeacherSubject.SubjectId,
                    SubjectName = tsg.TeacherSubject.Subject.Name,
                    GradeLevelName = tsg.TeacherSubject.Subject.GradeLevel.Name
                })
                .ToListAsync();

            ViewBag.Classes = classes;
            return View();
        }

        // POST: /Grades/BulkGrade/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int groupId, int subjectId, IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Error"] = "Selecciona un archivo";
                return RedirectToAction("SelectClass");
            }

            // Validar extensión
            var extension = Path.GetExtension(excelFile.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls" && extension != ".csv")
            {
                TempData["Error"] = "Solo se permiten archivos Excel o CSV";
                return RedirectToAction("SelectClass");
            }

            try
            {
                // Leer Excel con MiniExcel
                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;

                // Obtener filas como diccionarios
                var rows = stream.Query().ToList();

                if (!rows.Any())
                {
                    TempData["Error"] = "El archivo está vacío";
                    return RedirectToAction("SelectClass");
                }

                // Obtener headers (primer fila)
                var firstRow = (IDictionary<string, object>)rows[0];
                var headers = firstRow.Keys.ToList();

                // Obtener filas de previsualización (máximo 5)
                var previewRows = new List<Dictionary<string, object>>();
                for (int i = 0; i < Math.Min(rows.Count, 5); i++)
                {
                    previewRows.Add((Dictionary<string, object>)rows[i]);
                }

                // Obtener info del grupo y materia para la vista
                var group = await _context.grades_GradeGroups
                    .FirstOrDefaultAsync(g => g.GroupId == groupId);

                var subject = await _context.grades_Subjects
                    .FirstOrDefaultAsync(s => s.SubjectId == subjectId);

                var viewModel = new ExcelUploadViewModel
                {
                    GroupId = groupId,
                    SubjectId = subjectId,
                    GroupName = group?.Name ?? "",
                    SubjectName = subject?.Name ?? "",
                    FileName = excelFile.FileName,
                    Headers = headers,
                    PreviewRows = previewRows
                };

                return View("MapColumns", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al leer el archivo: {ex.Message}";
                return RedirectToAction("SelectClass");
            }
        }

        // POST: /Grades/BulkGrade/ProcessMapping
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessMapping(ProcessMappingViewModel model, IFormFile excelFile)
        {
            // Recuperar el archivo (en un entorno real, lo tendrías que guardar temporalmente)
            // Por ahora, asumimos que el mapping es suficiente y redirigimos

            TempData["Success"] = $"Mapeo recibido. Columna nombres: {model.NombreColumnIndex}, " +
                                  $"Unidades: {string.Join(", ", model.UnitColumns.Select(u => $"U{u.Key}->Col{u.Value}"))}";

            return RedirectToAction("SelectClass");
        }

        private int GetCurrentTeacherId()
        {
            // Temporal para pruebas
            return 4;
        }
    }
}