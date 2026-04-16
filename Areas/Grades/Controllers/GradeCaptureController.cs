using Azure.Core;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using SchoolManager.Areas.Grades.ViewModels.GradeCapture;
using SchoolManager.Data;
using SchoolManager.Grades.Services;
using SchoolManager.Helpers;
using SchoolManager.Models;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    [Authorize(Roles = "Teacher")]

    public class GradeCaptureController : Controller
    {
        private readonly ITeacherAccessService _access;
        private readonly AppDbContext _context;


        public GradeCaptureController(AppDbContext context, ITeacherAccessService access)
        {
            _context = context;
            _access = access;
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
                .Where(tsg => tsg.TeacherSubject.TeacherId == teacherId
                    && tsg.TeacherSubject.Subject.GradeLevel.IsOpen)
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
            var teacherId = GetCurrentTeacherId();
            if (!await _access.OwnsTeacherSubjectGroupAsync(teacherId, teacherSubjectGroupId))
                return Forbid();
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
            var teacherId = GetCurrentTeacherId();
            if (!await _access.OwnsTeacherSubjectGroupAsync(teacherId, teacherSubjectGroupId))
                return Forbid();
            var tsg = await _context.grades_TeacherSubjectGroups
                .Include(tsg => tsg.TeacherSubject)
                    .ThenInclude(ts => ts.Subject)
                .Include(tsg => tsg.Group)
                    .ThenInclude(g => g.Enrollments)
                        .ThenInclude(e => e.Student)
                            .ThenInclude(s => s.Person)
                .Include(tsg => tsg.Group)
                    .ThenInclude(g => g.Enrollments)
                        .ThenInclude(e => e.Student)
                            .ThenInclude(s => s.Preenrollments)
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
                    // s = e.Student (users_user)
                    Matricula = s.Preenrollments
                        .Where(p => p.Matricula != null)
                        .OrderByDescending(p => p.CreateStat)
                        .Select(p => p.Matricula)
                        .FirstOrDefault() ?? "S/N",
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
            var teacherId = GetCurrentTeacherId();
            if (!await _access.OwnsTeacherSubjectGroupAsync(teacherId, viewModel.TeacherSubjectGroupId))
                return Forbid();
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
            await TryAutoCalculateFinalGrades(viewModel.GroupId, unit.SubjectId, viewModel.UnitId);
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
            var teacherId = GetCurrentTeacherId();
            if (!await _access.OwnsGradeAsync(teacherId, gradeId))
                return Forbid();
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

            var unit = await _context.grades_SubjectUnits
                .FirstOrDefaultAsync(u => u.UnitId == unitId);
            if (unit != null)
                await TryAutoCalculateFinalGrades(grade.GroupId, unit.SubjectId, unitId);

            return RedirectToAction(nameof(Capture), new { teacherSubjectGroupId, unitId });
        }

        // POST: GradeCapture/DeleteRecovery/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecovery(int recoveryId, int teacherSubjectGroupId, int unitId)
        {
            var teacherId = GetCurrentTeacherId();
            if (!await _access.OwnsRecoveryAsync(teacherId, recoveryId))
                return Forbid();
            var recovery = await _context.grades_UnitRecoveries
                .Include(r => r.Grade)
                .FirstOrDefaultAsync(r => r.UnitRecoveryId == recoveryId);

            if (recovery == null) return NotFound();

            var groupId = recovery.Grade.GroupId;

            _context.grades_UnitRecoveries.Remove(recovery);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Recuperación eliminada correctamente";
            var unit = await _context.grades_SubjectUnits
                .FirstOrDefaultAsync(u => u.UnitId == unitId);
            if (unit != null)
                await TryAutoCalculateFinalGrades(groupId, unit.SubjectId, unitId);
            return RedirectToAction(nameof(Capture), new { teacherSubjectGroupId, unitId });
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(int teacherSubjectGroupId, int unitId, IFormFile excelFile)
        {
            var teacherId = GetCurrentTeacherId();
            if (!await _access.OwnsTeacherSubjectGroupAsync(teacherId, teacherSubjectGroupId))
                return Forbid();
            if (excelFile == null || excelFile.Length == 0)
                return BadRequest(new { error = "Selecciona un archivo" });

            var extension = Path.GetExtension(excelFile.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls" && extension != ".csv")
                return BadRequest(new { error = "Solo se permiten archivos .xlsx, .xls o .csv" });

            try
            {
                // 1. Leer a memoria
                using var ms = new MemoryStream();
                await excelFile.CopyToAsync(ms);
                var bytes = ms.ToArray();

                // 2. Guardar en disco temporal con GUID (elimina dependencia de TempData)
                var fileId = Guid.NewGuid().ToString();
                var tempDir = Path.Combine(Path.GetTempPath(), "SchoolManager", "ExcelImport");
                Directory.CreateDirectory(tempDir);
                var tempFilePath = Path.Combine(tempDir, $"{fileId}{extension}");
                await System.IO.File.WriteAllBytesAsync(tempFilePath, bytes);

                // 3. Leer headers y vista previa
                ms.Position = 0;
                var rows = ms.Query().ToList();

                if (!rows.Any())
                    return BadRequest(new { error = "El archivo está vacío" });

                var firstRow = (IDictionary<string, object>)rows[0];
                var headers = firstRow.Keys.ToList(); // ["A", "B", "C", ...]

                // Vista previa (máx 5 filas)
                var preview = new List<Dictionary<string, object>>();
                for (int i = 0; i < Math.Min(rows.Count, 5); i++)
                {
                    var row = (IDictionary<string, object>)rows[i];
                    preview.Add(row.ToDictionary(k => k.Key, k => k.Value));
                }

                return Ok(new
                {
                    headers,
                    preview,
                    fileName = excelFile.FileName,
                    fileId,          // ← El frontend lo envía de vuelta en ApplyExcelMapping
                    totalRows = rows.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Error al leer el archivo: {ex.Message}" });
            }
        }
        // POST: GradeCapture/ApplyExcelMapping
        [HttpPost]
        public async Task<IActionResult> ApplyExcelMapping([FromBody] ExcelImportViewModel model)
        {
            var teacherId = GetCurrentTeacherId();
            if (!await _access.OwnsTeacherSubjectGroupAsync(teacherId, model.TeacherSubjectGroupId))
                return BadRequest(new { error = "No tienes acceso a este grupo" });
            // ── Validaciones ─────────────────────────────────────────────
            if (model.NombreColumnIndex < 0)
                return BadRequest(new { error = "Debes seleccionar la columna de nombres" });

            if (model.CalificacionColumnIndex < 0)
                return BadRequest(new { error = "Debes seleccionar la columna de calificaciones" });

            if (model.NombreColumnIndex == model.CalificacionColumnIndex)
                return BadRequest(new { error = "Las columnas de nombre y calificación deben ser diferentes" });

            // ── Buscar archivo temporal por fileId ────────────────────────
            var tempDir = Path.Combine(Path.GetTempPath(), "SchoolManager", "ExcelImport");
            var tempFiles = Directory.Exists(tempDir)
                ? Directory.GetFiles(tempDir, $"{model.FileId}.*")
                : Array.Empty<string>();

            if (!tempFiles.Any())
                return BadRequest(new { error = "Archivo temporal no encontrado. Sube el archivo de nuevo." });

            var tempFilePath = tempFiles.First();

            try
            {
                // ── Cargar estudiantes del grupo ──────────────────────────
                var estudiantesBase = await _context.grades_Enrollments
                    .Include(e => e.Student).ThenInclude(s => s.Person)
                    .Where(e => e.GroupId == model.GroupId && e.IsActive)
                    .Select(e => new
                    {
                        StudentId = e.Student.UserId,
                        Nombre = (e.Student.Person.FirstName + " " +
                                  e.Student.Person.LastNamePaternal + " " +
                                  e.Student.Person.LastNameMaternal).Trim()
                    })
                    .ToListAsync();

                if (!estudiantesBase.Any())
                    return BadRequest(new { error = "No hay estudiantes inscritos en este grupo" });

                // Diccionario: nombre normalizado → studentId
                var porNombreNormalizado = new Dictionary<string, int>();
                var errores = new List<string>();

                foreach (var est in estudiantesBase)
                {
                    var key = NormalizarNombre(est.Nombre);
                    if (!porNombreNormalizado.ContainsKey(key))
                        porNombreNormalizado[key] = est.StudentId;
                    else
                        errores.Add($"Advertencia: nombre duplicado en grupo: '{est.Nombre}'");
                }

                // ── Leer Excel ────────────────────────────────────────────
                using var stream = System.IO.File.OpenRead(tempFilePath);
                var rows = stream.Query().ToList();

                var gradesToSave = new List<ImportedGradeResult>();
                int startRow = model.HasHeaderRow ? 1 : 0; // Si tiene encabezado, saltar fila 0

                for (int i = startRow; i < rows.Count; i++)
                {
                    var row = (IDictionary<string, object>)rows[i];
                    var values = row.Values.ToList();
                    int displayRow = i + 1; // Número de fila para mensajes (1-based)

                    // Leer nombre
                    var nombreExcel = values.ElementAtOrDefault(model.NombreColumnIndex)?
                        .ToString()?.Trim() ?? "";

                    if (string.IsNullOrEmpty(nombreExcel))
                    {
                        errores.Add($"Fila {displayRow}: nombre vacío, se omitió");
                        continue;
                    }

                    // Buscar estudiante por nombre normalizado
                    var nombreNorm = NormalizarNombre(nombreExcel);
                    if (!porNombreNormalizado.TryGetValue(nombreNorm, out int studentId))
                    {
                        errores.Add($"Fila {displayRow}: '{nombreExcel}' no encontrado en el grupo");
                        continue;
                    }

                    // Leer calificación
                    var califText = values.ElementAtOrDefault(model.CalificacionColumnIndex)?
                        .ToString()?.Trim() ?? "";

                    if (string.IsNullOrEmpty(califText))
                    {
                        errores.Add($"Fila {displayRow}: calificación vacía para '{nombreExcel}'");
                        continue;
                    }

                    // Parsear (soportar punto y coma como separador decimal)
                    califText = califText.Replace(',', '.');
                    if (decimal.TryParse(califText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal calif))
                    {
                        if (calif < 0 || calif > 10)
                        {
                            errores.Add($"Fila {displayRow}: '{calif}' fuera de rango (0-10)");
                            continue;
                        }

                        // Verificar duplicado (mismo estudiante 2 veces en Excel)
                        if (gradesToSave.Any(g => g.StudentId == studentId))
                        {
                            errores.Add($"Fila {displayRow}: '{nombreExcel}' duplicado en el archivo, se usó la primera aparición");
                            continue;
                        }

                        gradesToSave.Add(new ImportedGradeResult
                        {
                            StudentId = studentId,
                            Grade = calif
                        });
                    }
                    else
                    {
                        errores.Add($"Fila {displayRow}: '{califText}' no es un número válido");
                    }
                }

                return Ok(new
                {
                    grades = gradesToSave,
                    errores,
                    total = gradesToSave.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Error al procesar el archivo: {ex.Message}" });
            }
            finally
            {
                // Limpiar archivo temporal
                try { System.IO.File.Delete(tempFilePath); } catch { }
            }
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
            return User.GetUserId();
        }
        private async Task TryAutoCalculateFinalGrades(int groupId, int subjectId, int savedUnitId)
        {
            // 1. Obtener todas las unidades de la materia
            var allUnits = await _context.grades_SubjectUnits
                .Where(u => u.SubjectId == subjectId)
                .ToListAsync();

            if (!allUnits.Any()) return;

            // 2. Verificar que todas las unidades tienen calificaciones para todos los estudiantes del grupo
            var enrolledStudentIds = await _context.grades_Enrollments
                .Where(e => e.GroupId == groupId)
                .Select(e => e.StudentId)
                .ToListAsync();

            if (!enrolledStudentIds.Any()) return;

            var unitIds = allUnits.Select(u => u.UnitId).ToList();

            var gradedCombinations = await _context.grades_Grades
                .Where(g => g.GroupId == groupId && unitIds.Contains(g.SubjectUnitId))
                .Select(g => new { g.StudentId, g.SubjectUnitId })
                .ToListAsync();

            // Verificar que cada alumno tiene calificación en cada unidad
            bool allGraded = enrolledStudentIds.All(studentId =>
                unitIds.All(unitId =>
                    gradedCombinations.Any(g => g.StudentId == studentId && g.SubjectUnitId == unitId)
                )
            );

            if (!allGraded) return;

            // 3. Obtener nivel para calificación mínima
            var subject = await _context.grades_Subjects
                .Include(s => s.GradeLevel)
                .FirstOrDefaultAsync(s => s.SubjectId == subjectId);

            if (subject == null) return;

            var minPassingGrade = subject.MinPassingGrade ?? subject.GradeLevel.MinPassingGrade;
            if (minPassingGrade == 0) minPassingGrade = 6.0m;

            // 4. Obtener todas las calificaciones con sus recuperaciones
            var allGrades = await _context.grades_Grades
                .Include(g => g.Recoveries)
                .Where(g => g.GroupId == groupId && unitIds.Contains(g.SubjectUnitId))
                .ToListAsync();

            var existingFinals = await _context.grades_FinalGrades
                .Where(f => f.GroupId == groupId && f.SubjectId == subjectId)
                .ToDictionaryAsync(f => f.StudentId);

            // 5. Calcular y guardar para cada alumno
            foreach (var studentId in enrolledStudentIds)
            {
                var studentGrades = allGrades.Where(g => g.StudentId == studentId).ToList();
                var gradeValues = studentGrades.Select(g =>
                    g.Recoveries != null && g.Recoveries.Any()
                        ? Math.Max(g.Value, g.Recoveries.First().Value)
                        : g.Value
                ).ToList();

                while (gradeValues.Count < allUnits.Count) gradeValues.Add(0);

                var finalValue = gradeValues.Average();

                if (existingFinals.TryGetValue(studentId, out var existing) && existing.ExtraordinaryGrade != null)
                {
                    // No sobreescribir si ya tiene extraordinario asignado
                    existing.Value = finalValue;
                    existing.Passed = existing.ExtraordinaryGrade.Value >= minPassingGrade;
                }
                else if (existing != null)
                {
                    existing.Value = finalValue;
                    existing.Passed = finalValue >= minPassingGrade;
                    existing.MinPassingGradeUsed = minPassingGrade;
                    existing.CreatedAt = DateTime.Now;
                }
                else
                {
                    _context.grades_FinalGrades.Add(new grades_final_grades
                    {
                        StudentId = studentId,
                        SubjectId = subjectId,
                        GroupId = groupId,
                        Value = finalValue,
                        Passed = finalValue >= minPassingGrade,
                        MinPassingGradeUsed = minPassingGrade,
                        CalculationMethod = "Automático al completar unidades",
                        ConsideredRecoveries = true,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Calificaciones guardadas. ¡Se calcularon las finales automáticamente al completar todas las unidades!";
        }
    }
}   