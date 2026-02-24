using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.Areas.Grades.ViewModels.GradeCapture;
using System.Security.Claims;

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
        private int GetCurrentTeacherId()
        {
            //Propuesta para obtener el ID del profesor logueado usando claims
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // return int.Parse(userId);

            // TEMPORAL PARA PRUEBAS ESTATICO CON VALOR 5
            return 9; 
        }
    }
}   