using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Grades.ViewModels.StudentGrades;
using SchoolManager.Data;
using SchoolManager.Helpers;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    public class StudentGradesController : Controller
    {
        private readonly AppDbContext _context;

        public StudentGradesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: StudentGrades/MyGrades
#pragma warning disable CS1998 // El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica
        public async Task<IActionResult> MyGrades()
#pragma warning restore CS1998 // El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica
        {
            int studentId = User.GetUserId();

            return RedirectToAction(nameof(Details), new { studentId });
        }

        // GET: StudentGrades/Details/5
        public async Task<IActionResult> Details(int studentId)
        {
            // 1. Obtener información del estudiante (SIN Enrollments)
            var student = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == studentId);

            if (student == null)
            {
                return NotFound("Estudiante no encontrado");
            }

            // 2. Obtener inscripciones del estudiante por separado
            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

            // 3. Obtener el grupo actual (el primero activo, por ejemplo)
            var currentEnrollment = enrollments.FirstOrDefault();

            if (currentEnrollment == null)
            {
                ViewBag.Message = "No estás inscrito en ningún grupo";
                return View("NoEnrollment");
            }

            var group = currentEnrollment.Group;
            var gradeLevel = group.GradeLevel;

            // 4. Resto del código igual...
            var subjects = await _context.grades_Subjects
                .Include(s => s.Units)
                .Where(s => s.GradeLevelId == gradeLevel.GradeLevelId)
                .ToListAsync();

            var unitGrades = await _context.grades_Grades
                .Include(g => g.Recoveries)
                .Include(g => g.SubjectUnit)
                .Where(g => g.StudentId == studentId && g.GroupId == group.GroupId)
                .ToListAsync();

            // 5. Obtener calificaciones finales
            var finalGrades = await _context.grades_FinalGrades
                .Include(f => f.ExtraordinaryGrade)
                .Where(f => f.StudentId == studentId && f.GroupId == group.GroupId)
                .ToDictionaryAsync(f => f.SubjectId);

            // 6. Construir vista modelo
            var subjectGrades = new List<StudentSubjectGradeViewModel>();
            int aprobadas = 0, reprobadas = 0, extraordinarios = 0;
            decimal sumaPromedios = 0;

            foreach (var subject in subjects)
            {
                // Calificaciones por unidad
                var subjectUnitGrades = unitGrades
                    .Where(g => g.SubjectUnit.SubjectId == subject.SubjectId)
                    .ToList();

                var unitViewModels = subject.Units
                    .OrderBy(u => u.UnitNumber)
                    .Select(u =>
                    {
                        var grade = subjectUnitGrades.FirstOrDefault(g => g.SubjectUnitId == u.UnitId);
                        return new UnitGradeViewModel
                        {
                            UnitNumber = u.UnitNumber,
                            Grade = grade?.Value,
                            Recovery = grade?.Recoveries.FirstOrDefault()?.Value
                        };
                    }).ToList();

                // Calificación final
                finalGrades.TryGetValue(subject.SubjectId, out var final);

                var subjectVm = new StudentSubjectGradeViewModel
                {
                    SubjectName = subject.Name,
                    Units = unitViewModels,
                    FinalGrade = final?.Value,
                    Passed = final?.Passed ?? false,
                    HasExtraordinary = final?.ExtraordinaryGrade != null,
                    ExtraordinaryGrade = final?.ExtraordinaryGrade?.Value
                };

                subjectGrades.Add(subjectVm);

                // Estadísticas
                if (subjectVm.HasExtraordinary)
                    extraordinarios++;
                else if (subjectVm.Passed)
                    aprobadas++;
                else
                    reprobadas++;

                if (final?.Value > 0)
                    sumaPromedios += final.Value;
            }

            var viewModel = new StudentDashboardViewModel
            {
                StudentId = student.UserId,
                StudentName = $"{student.Person.FirstName} {student.Person.LastNamePaternal} {student.Person.LastNameMaternal}",
                Matricula = student.Person.Curp ?? "S/N",
                GradeLevel = gradeLevel.Name,
                GroupName = group.Name,
                Subjects = subjectGrades,
                Resumen = new ResumenViewModel
                {
                    TotalMaterias = subjects.Count,
                    Aprobadas = aprobadas,
                    Reprobadas = reprobadas,
                    Extraordinarios = extraordinarios,
                    PromedioGeneral = subjectGrades.Count > 0 ? sumaPromedios / subjectGrades.Count : 0
                }
            };

            return View(viewModel);
        }
    }
}