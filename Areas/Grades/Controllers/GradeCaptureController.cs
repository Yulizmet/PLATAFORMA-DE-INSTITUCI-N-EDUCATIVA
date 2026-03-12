using Azure.Core;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using SchoolManager.Areas.Grades.ViewModels.GradeCapture;
using SchoolManager.Data;
using SchoolManager.Models;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    public class GradeCaptureController : Controller
    {
        private readonly AppDbContext _context;

        public GradeCaptureController(AppDbContext context)
        {
            _context = context;
        }

        // GET: GradeCapture/MyClasses
        public async Task<IActionResult> MyClasses()
        {
            // Obtener el ID del profesor logueado (pendiente de usar sesiones aqui de momento es 5)
            var teacherId = GetCurrentTeacherId();

            var classes = await _context.grades_TeacherSubjectGroups
                .Include(tsg => tsg.TeacherSubject)
                    .ThenInclude(ts => ts.Subject)
                        .ThenInclude(s => s.GradeLevel)
                .Include(tsg => tsg.Group)
                .Where(tsg => tsg.TeacherSubject.TeacherId == teacherId)
                .Select(tsg => new TeacherClassViewModel
                {
                    TeacherSubjectGroupId = tsg.TeacherSubjectGroupId,
                    GroupName = tsg.Group.Name,
                    SubjectName = tsg.TeacherSubject.Subject.Name,
                    GradeLevelName = tsg.TeacherSubject.Subject.GradeLevel.Name,
                    GroupId = tsg.GroupId,
                    SubjectId = tsg.TeacherSubject.SubjectId
                })
                .ToListAsync();

            var viewModel = new TeacherClassSelectionViewModel
            {
                Classes = classes
            };

            return View(viewModel);
        }

        // GET: GradeCapture/SelectUnit/5
        public async Task<IActionResult> SelectUnit(int teacherSubjectGroupId)
        {
            var tsg = await _context.grades_TeacherSubjectGroups
                .Include(tsg => tsg.TeacherSubject)
                    .ThenInclude(ts => ts.Subject)
                        .ThenInclude(s => s.Units)
                .Include(tsg => tsg.Group)
                .FirstOrDefaultAsync(tsg => tsg.TeacherSubjectGroupId == teacherSubjectGroupId);

            if (tsg == null) return NotFound();

            var viewModel = new UnitSelectionViewModel
            {
                TeacherSubjectGroupId = teacherSubjectGroupId,
                GroupId = tsg.GroupId,
                GroupName = tsg.Group.Name,
                SubjectId = tsg.TeacherSubject.SubjectId,
                SubjectName = tsg.TeacherSubject.Subject.Name,
                Units = tsg.TeacherSubject.Subject.Units
                    .OrderBy(u => u.UnitNumber)
                    .Select(u => new UnitOptionViewModel
                    {
                        UnitId = u.UnitId,
                        UnitNumber = u.UnitNumber,
                        IsOpen = u.IsOpen,
                        HasGrades = _context.grades_Grades.Any(g => g.SubjectUnitId == u.UnitId)
                    }).ToList()
            };

            return View(viewModel);
        }

        // GET: GradeCapture/Capture
        public async Task<IActionResult> Capture(int teacherSubjectGroupId, int unitId)
        {
            var tsg = await _context.grades_TeacherSubjectGroups
                .Include(tsg => tsg.TeacherSubject)
                    .ThenInclude(ts => ts.Subject)
                .Include(tsg => tsg.Group)
                    .ThenInclude(g => g.Enrollments)
                        .ThenInclude(e => e.Student)
                            .ThenInclude(s => s.Person)
                .FirstOrDefaultAsync(tsg => tsg.TeacherSubjectGroupId == teacherSubjectGroupId);

            if (tsg == null) return NotFound();

            var unit = await _context.grades_SubjectUnits
                .FirstOrDefaultAsync(u => u.UnitId == unitId);

            if (unit == null) return NotFound();

            // Obtener calificaciones existentes para esta unidad
            var existingGrades = await _context.grades_Grades
                .Include(g => g.Recoveries)
                .Where(g => g.SubjectUnitId == unitId &&
                           g.GroupId == tsg.GroupId)
                .ToListAsync();

            var students = tsg.Group.Enrollments
                .Select(e => e.Student)
                .Select(s => new StudentGradeViewModel
                {
                    StudentId = s.UserId,
                    StudentName = $"{s.Person.FirstName} {s.Person.LastNamePaternal} {s.Person.LastNameMaternal}",
                    Matricula = s.Person.Curp ?? "S/N",
                    GradeId = existingGrades.FirstOrDefault(g => g.StudentId == s.UserId)?.GradeId,
                    GradeValue = existingGrades.FirstOrDefault(g => g.StudentId == s.UserId)?.Value,
                    HasRecovery = existingGrades.Any(g => g.StudentId == s.UserId && g.Recoveries.Any()),
                    RecoveryValue = existingGrades.FirstOrDefault(g => g.StudentId == s.UserId)?.Recoveries.FirstOrDefault()?.Value,
                    RecoveryId = existingGrades.FirstOrDefault(g => g.StudentId == s.UserId)?.Recoveries.FirstOrDefault()?.UnitRecoveryId
                })
                .OrderBy(s => s.StudentName)
                .ToList();

            var viewModel = new GradeCaptureViewModel
            {
                TeacherSubjectGroupId = teacherSubjectGroupId,
                GroupId = tsg.GroupId,
                GroupName = tsg.Group.Name,
                SubjectId = tsg.TeacherSubject.SubjectId,
                SubjectName = tsg.TeacherSubject.Subject.Name,
                UnitId = unitId,
                UnitNumber = unit.UnitNumber,
                Students = students
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrades(SaveGradesViewModel viewModel)
        {
            var unit = await _context.grades_SubjectUnits
                .FirstOrDefaultAsync(u => u.UnitId == viewModel.UnitId);

            if (unit == null) return NotFound();

            if (!unit.IsOpen)
            {
                TempData["Error"] = "Esta unidad está cerrada. No se pueden guardar calificaciones.";
                return RedirectToAction(nameof(SelectUnit), new { teacherSubjectGroupId = viewModel.TeacherSubjectGroupId });
            }

            foreach (var gradeInput in viewModel.Grades)
            {
                if (gradeInput.GradeValue.HasValue)
                {
                    var existingGrade = await _context.grades_Grades
                        .FirstOrDefaultAsync(g => g.StudentId == gradeInput.StudentId &&
                                                  g.SubjectUnitId == viewModel.UnitId);

                    if (existingGrade != null)
                    {
                        existingGrade.Value = gradeInput.GradeValue.Value;
                        existingGrade.CreatedAt = DateTime.Now;
                    }
                    else
                    {
                        var newGrade = new grades_grades
                        {
                            StudentId = gradeInput.StudentId,
                            GroupId = viewModel.GroupId,
                            SubjectUnitId = viewModel.UnitId,
                            Value = gradeInput.GradeValue.Value,
                            CreatedAt = DateTime.Now
                        };
                        _context.grades_Grades.Add(newGrade);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Calificaciones guardadas exitosamente";

            return RedirectToAction(nameof(Capture), new
            {
                teacherSubjectGroupId = viewModel.TeacherSubjectGroupId,
                unitId = viewModel.UnitId
            });
        }

        // POST: GradeCapture/SaveRecovery
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRecovery(int gradeId, decimal recoveryValue, int teacherSubjectGroupId, int unitId)
        {
            var grade = await _context.grades_Grades
                .Include(g => g.Recoveries)
                .FirstOrDefaultAsync(g => g.GradeId == gradeId);

            if (grade == null) return NotFound();

            var existingRecovery = grade.Recoveries.FirstOrDefault();

            if (existingRecovery != null)
            {
                existingRecovery.Value = recoveryValue;
                existingRecovery.CreatedAt = DateTime.Now;
            }
            else
            {
                var recovery = new grades_unit_recovery
                {
                    GradeId = gradeId,
                    Value = recoveryValue,
                    CreatedAt = DateTime.Now
                };
                _context.grades_UnitRecoveries.Add(recovery);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Recuperación guardada exitosamente";

            return RedirectToAction(nameof(Capture), new { teacherSubjectGroupId, unitId });
        }

        // POST: GradeCapture/DeleteRecovery/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecovery(int recoveryId, int teacherSubjectGroupId, int unitId)
        {
            var recovery = await _context.grades_UnitRecoveries
                .FirstOrDefaultAsync(r => r.UnitRecoveryId == recoveryId);

            if (recovery == null) return NotFound();

            _context.grades_UnitRecoveries.Remove(recovery);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Recuperación eliminada correctamente";
            return RedirectToAction(nameof(Capture), new { teacherSubjectGroupId, unitId });
        }
        public class ImportedGrade
        {
            public int StudentId { get; set; }
            public decimal Grade { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> UploadExcel(int teacherSubjectGroupId, int unitId, IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
                return BadRequest(new { error = "Selecciona un archivo" });

            var extension = Path.GetExtension(excelFile.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls" && extension != ".csv")
                return BadRequest(new { error = "Solo se permiten archivos .xlsx, .xls o .csv" });

            try
            {
                // Guardar en disco temporal
                var tempPath = Path.Combine(Path.GetTempPath(), "SchoolManager", "GradeCapture");
                Directory.CreateDirectory(tempPath);
                var tempFileName = $"{Guid.NewGuid()}_{excelFile.FileName}";
                var tempFilePath = Path.Combine(tempPath, tempFileName);

                using (var stream = System.IO.File.Create(tempFilePath))
                    await excelFile.CopyToAsync(stream);

                // Leer headers y preview
                using var ms = new MemoryStream();
                await excelFile.CopyToAsync(ms);
                ms.Position = 0;

                var rows = ms.Query().ToList();
                if (!rows.Any())
                    return BadRequest(new { error = "El archivo está vacío" });

                var firstRow = (IDictionary<string, object>)rows[0];
                var headers = firstRow.Keys.ToList();

                var preview = new List<Dictionary<string, object>>();
                for (int i = 0; i < Math.Min(rows.Count, 3); i++)
                {
                    var row = (IDictionary<string, object>)rows[i];
                    preview.Add(row.ToDictionary(k => k.Key, k => k.Value));
                }

                // Guardar SOLO la ruta en TempData (no headers ni preview)
                TempData["TempExcelPath"] = tempFilePath;

                return Ok(new { headers, preview, fileName = excelFile.FileName });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Error al leer el archivo: {ex.Message}" });
            }
        }

        // POST: GradeCapture/ApplyExcelMapping
        [HttpPost]
        public async Task<IActionResult> ApplyExcelMapping([FromBody] ExcelMappingViewModel model)
        {
            // Construir índices desde ColumnMappings
            foreach (var mapping in model.ColumnMappings)
            {
                if (mapping.Type == "nombre")
                    model.NombreColumnIndex = mapping.ColumnIndex;
                else if (mapping.Type == "calificacion" && mapping.UnitNumber.HasValue)
                    model.UnitColumns[mapping.UnitNumber.Value] = mapping.ColumnIndex;
            }

            if (model.NombreColumnIndex == -1)
                return BadRequest(new { error = "Debes seleccionar una columna para los nombres" });

            if (!model.UnitColumns.ContainsKey(model.UnitId))
                return BadRequest(new { error = "Debes seleccionar una columna de calificación para esta unidad" });

            var tempFilePath = TempData["TempExcelPath"]?.ToString();
            if (string.IsNullOrEmpty(tempFilePath) || !System.IO.File.Exists(tempFilePath))
                return BadRequest(new { error = "Archivo no encontrado. Intenta de nuevo." });

            // Cargar estudiantes
            var estudiantesBase = await _context.grades_Enrollments
                .Include(e => e.Student).ThenInclude(s => s.Person)
                .Where(e => e.GroupId == model.GroupId)
                .Select(e => new
                {
                    StudentId = e.Student.UserId,
                    Nombre = e.Student.Person.FirstName + " " +
                             e.Student.Person.LastNamePaternal + " " +
                             e.Student.Person.LastNameMaternal
                })
                .ToListAsync();

            var estudiantesGrupo = estudiantesBase.Select(e => new
            {
                e.StudentId,
                e.Nombre,
                NombreNormalizado = NormalizarNombre(e.Nombre)
            }).ToList();

            var porNombreOriginal = estudiantesGrupo.ToDictionary(e => e.Nombre, e => e.StudentId);
            var porNombreNormalizado = estudiantesGrupo.ToDictionary(e => e.NombreNormalizado, e => e.StudentId);

            using var stream = System.IO.File.OpenRead(tempFilePath);
            var rows = stream.Query().ToList();

            var gradesToSave = new List<ImportedGrade>();
            var errores = new List<string>();

            for (int i = 1; i < rows.Count; i++)
            {
                var row = (IDictionary<string, object>)rows[i];
                var nombreExcel = row.ElementAt(model.NombreColumnIndex).Value?.ToString()?.Trim() ?? "";
                var califText = row.ElementAt(model.UnitColumns[model.UnitId]).Value?.ToString()?.Trim() ?? "";

                if (string.IsNullOrEmpty(nombreExcel)) { errores.Add($"Fila {i + 1}: Nombre vacío"); continue; }

                int studentId;
                if (!porNombreOriginal.TryGetValue(nombreExcel, out studentId) &&
                    !porNombreNormalizado.TryGetValue(NormalizarNombre(nombreExcel), out studentId))
                {
                    errores.Add($"Fila {i + 1}: '{nombreExcel}' no encontrado en el grupo");
                    continue;
                }

                if (string.IsNullOrEmpty(califText)) continue;

                if (decimal.TryParse(califText, out decimal calif) && calif >= 0 && calif <= 10)
                    gradesToSave.Add(new ImportedGrade { StudentId = studentId, Grade = calif });
                else
                    errores.Add($"Fila {i + 1}: '{califText}' no es válida (rango 0-10)");
            }

            // Limpiar archivo temporal
            try { System.IO.File.Delete(tempFilePath); } catch { }
            TempData.Remove("TempExcelPath");

            return Ok(new { grades = gradesToSave, errores, total = gradesToSave.Count });
        }
        private string NormalizarNombre(string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return "";

            // Convertir a minúsculas y quitar espacios extras
            var normalizado = nombre.Trim().ToLower();

            // Quitar acentos usando NormalizationForm
            normalizado = new string(normalizado
                .Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray())
                .Replace('ñ', 'n');

            // Quitar espacios múltiples
            while (normalizado.Contains("  "))
                normalizado = normalizado.Replace("  ", " ");

            return normalizado;
        }
        private int GetCurrentTeacherId()
        {
            //Propuesta para obtener el ID del profesor logueado usando claims
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // return int.Parse(userId);

            // TEMPORAL PARA PRUEBAS ESTATICO CON VALOR 4
            return 4; 
        }
    }
}   