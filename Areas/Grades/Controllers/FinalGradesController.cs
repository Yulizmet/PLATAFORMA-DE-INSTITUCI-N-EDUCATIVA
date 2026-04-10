using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Grades.ViewModels.FinalGrades;
using SchoolManager.Data;
using SchoolManager.Helpers;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    [Authorize(Roles = "Teacher")]

    public class FinalGradesController : Controller
    {
        private readonly AppDbContext _context;

        public FinalGradesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: FinalGrades/Index
        public async Task<IActionResult> Index()
        {
            var teacherId = User.GetUserId();

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

            return View(classes);
        }
        // GET: FinalGrades/Details/5
        public async Task<IActionResult> Details(int groupId, int subjectId)
        {
            var group = await _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .Include(g => g.Enrollments)
                    .ThenInclude(e => e.Student)
                        .ThenInclude(s => s.Person)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null) return NotFound();

            var subject = await _context.grades_Subjects
                .Include(s => s.Units)
                .FirstOrDefaultAsync(s => s.SubjectId == subjectId);

            if (subject == null) return NotFound();

            // Obtener calificaciones de unidades
            var unitGrades = await _context.grades_Grades
                .Include(g => g.Recoveries)
                .Include(g => g.SubjectUnit)
                .Where(g => g.GroupId == groupId &&
                           g.SubjectUnit.SubjectId == subjectId)
                .ToListAsync();

            // Obtener calificaciones finales existentes
            var finalGrades = await _context.grades_FinalGrades
                .Include(f => f.ExtraordinaryGrade)
                .Where(f => f.GroupId == groupId && f.SubjectId == subjectId)
                .ToDictionaryAsync(f => f.StudentId);

            // Calcular calificación mínima a usar
            var minPassingGrade = subject.MinPassingGrade ?? group.GradeLevel.MinPassingGrade;
            if (minPassingGrade == 0) minPassingGrade = 6.0m;

            var students = group.Enrollments
                .Select(e => e.Student)
                .Select(s =>
                {
                    var studentUnitGrades = unitGrades
                        .Where(g => g.StudentId == s.UserId)
                        .ToList();

                    var finalGrade = finalGrades.GetValueOrDefault(s.UserId);

                    return new FinalGradeStudentViewModel
                    {
                        StudentId = s.UserId,
                        StudentName = $"{s.Person.FirstName} {s.Person.LastNamePaternal} {s.Person.LastNameMaternal}",
                        Matricula = s.Preenrollments
                            .Where(p => p.Matricula != null)
                            .OrderByDescending(p => p.CreateStat)
                            .Select(p => p.Matricula)
                            .FirstOrDefault() ?? "S/N",
                        FinalGrade = finalGrade?.Value,
                        Passed = finalGrade?.Passed ?? false,
                        FinalGradeId = finalGrade?.FinalGradeId,
                        HasExtraordinary = finalGrade?.ExtraordinaryGrade != null,
                        ExtraordinaryGrade = finalGrade?.ExtraordinaryGrade?.Value,
                        ExtraordinaryGradeId = finalGrade?.ExtraordinaryGrade?.ExtraordinaryGradeId
                    };
                })
                .OrderBy(s => s.StudentName)
                .ToList();

            var viewModel = new FinalGradeListViewModel
            {
                GroupId = groupId,
                GroupName = group.Name,
                SubjectId = subjectId,
                SubjectName = subject.Name,
                GradeLevelName = group.GradeLevel.Name,
                MinPassingGradeUsed = minPassingGrade,
                Students = students
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(int groupId, int subjectId)
        {
            // 1. Validar grupo
            var group = await _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .Include(g => g.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
            {
                TempData["Error"] = "No se encontró el grupo";
                return RedirectToAction(nameof(Index));
            }

            // 2. Validar materia
            var subject = await _context.grades_Subjects
                .Include(s => s.Units)
                .FirstOrDefaultAsync(s => s.SubjectId == subjectId);

            if (subject == null)
            {
                TempData["Error"] = "No se encontró la materia";
                return RedirectToAction(nameof(Index));
            }

            // 3. Validar que el grupo tenga estudiantes
            if (group.Enrollments == null || !group.Enrollments.Any())
            {
                TempData["Error"] = "El grupo no tiene estudiantes inscritos";
                return RedirectToAction(nameof(Index));
            }

            // 4. Obtener calificaciones mínima
            var minPassingGrade = subject.MinPassingGrade ?? group.GradeLevel.MinPassingGrade;
            if (minPassingGrade == 0) minPassingGrade = 6.0m;

            // 5. Obtener calificaciones de unidades
            var unitGrades = await _context.grades_Grades
                .Include(g => g.Recoveries)
                .Include(g => g.SubjectUnit)
                .Where(g => g.GroupId == groupId &&
                           g.SubjectUnit.SubjectId == subjectId)
                .ToListAsync();

            // 6. Obtener calificaciones finales existentes
            var existingFinalGrades = await _context.grades_FinalGrades
                .Include(f => f.ExtraordinaryGrade)
                .Where(f => f.GroupId == groupId && f.SubjectId == subjectId)   
                .ToDictionaryAsync(f => f.StudentId);

            // 7. Calcular para cada estudiante
            foreach (var enrollment in group.Enrollments)
            {
                var student = enrollment.Student;
                if (student == null) continue;

                var studentUnitGrades = unitGrades?
                    .Where(g => g.StudentId == student.UserId)
                    .ToList() ?? new List<grades_grades>();

                var calculatedFinal = CalculateFinalGrade(studentUnitGrades, subject.Units?.Count ?? 0);

                // Determinar si pasa (considerando extraordinario si existe)
                bool passed;
                if (existingFinalGrades.TryGetValue(student.UserId, out var existing) && existing.ExtraordinaryGrade != null)
                {
                    // Si tiene extraordinario, usarlo para determinar si pasa
                    passed = existing.ExtraordinaryGrade.Value >= minPassingGrade;
                }
                else
                {
                    // Si no tiene extraordinario, usar el promedio calculado
                    passed = calculatedFinal >= minPassingGrade;
                }

                if (existing != null)
                {
                    existing.Value = calculatedFinal;
                    existing.Passed = passed;
                    existing.MinPassingGradeUsed = minPassingGrade;
                    existing.CalculationMethod = "Promedio con recuperaciones";
                    existing.ConsideredRecoveries = true;
                    existing.CreatedAt = DateTime.Now;
                    // El extraordinario no se modifica
                }
                else
                {
                    var finalGrade = new grades_final_grades
                    {
                        StudentId = student.UserId,
                        SubjectId = subjectId,
                        GroupId = groupId,
                        Value = calculatedFinal,
                        Passed = passed,
                        MinPassingGradeUsed = minPassingGrade,
                        CalculationMethod = "Promedio con recuperaciones",
                        ConsideredRecoveries = true,
                        CreatedAt = DateTime.Now
                    };
                    _context.grades_FinalGrades.Add(finalGrade);
                }
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Calificaciones calculadas (Mínimo: {minPassingGrade:F1})";

            return RedirectToAction(nameof(Details), new { groupId, subjectId });
        }

        private decimal CalculateFinalGrade(List<grades_grades> unitGrades, int totalUnits)
        {
            if (unitGrades == null || !unitGrades.Any()) return 0;

            var grades = new List<decimal>();

            foreach (var grade in unitGrades)
            {
                if (grade == null) continue;

                if (grade.Recoveries != null && grade.Recoveries.Any())
                {
                    var recovery = grade.Recoveries.First().Value;
                    grades.Add(Math.Max(grade.Value, recovery));
                }
                else
                {
                    grades.Add(grade.Value);
                }
            }

            while (grades.Count < totalUnits)
            {
                grades.Add(0);
            }

            return grades.Any() ? grades.Average() : 0;
        }

        // POST: FinalGrades/AddExtraordinary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExtraordinary(int finalGradeId, decimal extraordinaryValue)
        {
            var finalGrade = await _context.grades_FinalGrades
                .Include(f => f.ExtraordinaryGrade)
                .FirstOrDefaultAsync(f => f.FinalGradeId == finalGradeId);

            if (finalGrade == null) return NotFound();

            // Obtener la calificación mínima que se usó (de la final grade o calcularla)
            var minPassingGrade = finalGrade.MinPassingGradeUsed;

            if (finalGrade.ExtraordinaryGrade != null)
            {
                // Actualizar existente
                finalGrade.ExtraordinaryGrade.Value = extraordinaryValue;
                finalGrade.ExtraordinaryGrade.CreatedAt = DateTime.Now;
            }
            else
            {
                // Crear nuevo extraordinario
                var extraordinary = new grades_extraordinary_grades
                {
                    FinalGradeId = finalGradeId,
                    Value = extraordinaryValue,
                    CreatedAt = DateTime.Now
                };
                _context.grades_ExtraordinaryGrades.Add(extraordinary);
            }

            // Actualizar estado de aprobación (si el extraordinario alcanza el mínimo)
            finalGrade.Passed = extraordinaryValue >= minPassingGrade;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Calificación extraordinaria guardada: {extraordinaryValue:F2}";
            return RedirectToAction(nameof(Details), new
            {
                groupId = finalGrade.GroupId,
                subjectId = finalGrade.SubjectId
            });
        }

        // POST: FinalGrades/DeleteExtraordinary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExtraordinary(int extraordinaryId)
        {
            var extraordinary = await _context.grades_ExtraordinaryGrades
                .Include(e => e.FinalGrade)
                .FirstOrDefaultAsync(e => e.ExtraordinaryGradeId == extraordinaryId);

            if (extraordinary == null) return NotFound();

            var finalGrade = extraordinary.FinalGrade;
            var groupId = finalGrade.GroupId;
            var subjectId = finalGrade.SubjectId;

            // Recalcular si aprobaba sin extraordinario
            var minPassingGrade = finalGrade.MinPassingGradeUsed;
            finalGrade.Passed = finalGrade.Value >= minPassingGrade;

            _context.grades_ExtraordinaryGrades.Remove(extraordinary);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Extraordinario eliminado";
            return RedirectToAction(nameof(Details), new { groupId, subjectId });
        }

    }
}