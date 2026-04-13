// Areas/Grades/Controllers/KardexController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Grades.ViewModels.Kardex;
using SchoolManager.Data;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    [Authorize(Roles = "Administrator")]
    public class KardexController : Controller
    {
        private readonly AppDbContext _context;

        public KardexController(AppDbContext context)
        {
            _context = context;
        }

        // En KardexController.cs — agregar este método
        public async Task<IActionResult> Print(int studentId)
        {
            // Reutilizar la misma lógica de Details
            // (idealmente extraer a un método privado, pero por simplicidad)

            var student = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.Preenrollments)
                .FirstOrDefaultAsync(u => u.UserId == studentId);

            if (student == null) return NotFound();

            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

            var semesterData = enrollments
                .GroupBy(e => e.Group.GradeLevelId)
                .Select(g => new
                {
                    GradeLevel = g.First().Group.GradeLevel,
                    GroupName = g.OrderByDescending(e => e.EnrolledAt).First().Group.Name,
                    GroupId = g.OrderByDescending(e => e.EnrolledAt).First().GroupId
                })
                .OrderBy(s => s.GradeLevel.StartDate)
                .ToList();

            var semesters = new List<KardexSemesterViewModel>();

            foreach (var sem in semesterData)
            {
                var subjects = await _context.grades_Subjects
                    .Include(s => s.Units)
                    .Where(s => s.GradeLevelId == sem.GradeLevel.GradeLevelId)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                var unitGrades = await _context.grades_Grades
                    .Include(g => g.Recoveries)
                    .Include(g => g.SubjectUnit)
                    .Where(g => g.StudentId == studentId && g.GroupId == sem.GroupId)
                    .ToListAsync();

                var finalGrades = await _context.grades_FinalGrades
                    .Include(f => f.ExtraordinaryGrade)
                    .Where(f => f.StudentId == studentId && f.GroupId == sem.GroupId)
                    .ToDictionaryAsync(f => f.SubjectId);

                var subjectVMs = subjects.Select(subject =>
                {
                    var subjectUnitGrades = unitGrades
                        .Where(g => g.SubjectUnit.SubjectId == subject.SubjectId)
                        .ToList();

                    var units = subject.Units
                        .OrderBy(u => u.UnitNumber)
                        .Select(u =>
                        {
                            var grade = subjectUnitGrades.FirstOrDefault(g => g.SubjectUnitId == u.UnitId);
                            return new KardexUnitGradeViewModel
                            {
                                UnitNumber = u.UnitNumber,
                                Grade = grade?.Value,
                                Recovery = grade?.Recoveries?.FirstOrDefault()?.Value
                            };
                        }).ToList();

                    finalGrades.TryGetValue(subject.SubjectId, out var final_);

                    return new KardexSubjectViewModel
                    {
                        SubjectName = subject.Name,
                        Units = units,
                        FinalGrade = final_?.Value,
                        Passed = final_?.Passed ?? false,
                        HasExtraordinary = final_?.ExtraordinaryGrade != null,
                        ExtraordinaryGrade = final_?.ExtraordinaryGrade?.Value
                    };
                }).ToList();

                var withFinal = subjectVMs.Where(s => s.FinalGrade.HasValue).ToList();

                semesters.Add(new KardexSemesterViewModel
                {
                    GradeLevelId = sem.GradeLevel.GradeLevelId,
                    GradeLevelName = sem.GradeLevel.Name,
                    GroupName = sem.GroupName,
                    StartDate = sem.GradeLevel.StartDate,
                    EndDate = sem.GradeLevel.EndDate,
                    IsOpen = sem.GradeLevel.IsOpen,
                    Subjects = subjectVMs,
                    PromedioSemestre = withFinal.Any()
                        ? withFinal.Average(s => s.FinalGrade!.Value) : 0
                });
            }

            var allSubjects = semesters.SelectMany(s => s.Subjects).ToList();
            var allWithFinal = allSubjects.Where(s => s.FinalGrade.HasValue).ToList();

            var matricula = student.Preenrollments?
                .Where(p => p.Matricula != null)
                .OrderByDescending(p => p.CreateStat)
                .Select(p => p.Matricula)
                .FirstOrDefault() ?? "S/N";

            var viewModel = new KardexViewModel
            {
                StudentId = student.UserId,
                StudentName = $"{student.Person.FirstName} {student.Person.LastNamePaternal} {student.Person.LastNameMaternal}",
                Matricula = matricula,
                Email = student.Email,
                Semesters = semesters,
                PromedioGeneral = allWithFinal.Any()
                    ? allWithFinal.Average(s => s.FinalGrade!.Value) : 0,
                TotalMaterias = allSubjects.Count,
                MateriasAprobadas = allSubjects.Count(s => s.Passed && !s.HasExtraordinary),
                MateriasReprobadas = allSubjects.Count(s => !s.Passed),
                MateriasConExtraordinario = allSubjects.Count(s => s.HasExtraordinary)
            };

            return View(viewModel);
        }

        // GET: /Grades/Kardex
        public async Task<IActionResult> Index()
        {
            var students = await _context.UserRoles
                .Include(ur => ur.User)
                    .ThenInclude(u => u.Person)
                .Include(ur => ur.User)
                    .ThenInclude(u => u.Preenrollments)
                .Where(ur => ur.Role.Name == "Student" && ur.IsActive)
                .Select(ur => new KardexStudentSearchViewModel
                {
                    StudentId = ur.User.UserId,
                    FullName = ur.User.Person.FirstName + " " +
                               ur.User.Person.LastNamePaternal + " " +
                               ur.User.Person.LastNameMaternal,
                    Matricula = ur.User.Preenrollments
                        .Where(p => p.Matricula != null)
                        .OrderByDescending(p => p.CreateStat)
                        .Select(p => p.Matricula)
                        .FirstOrDefault() ?? "S/N",
                    Email = ur.User.Email,
                    CurrentGroup = _context.grades_Enrollments
                        .Where(e => e.StudentId == ur.User.UserId && e.IsActive)
                        .Select(e => e.Group.Name)
                        .FirstOrDefault(),
                    CurrentGradeLevel = _context.grades_Enrollments
                        .Where(e => e.StudentId == ur.User.UserId && e.IsActive)
                        .Select(e => e.Group.GradeLevel.Name)
                        .FirstOrDefault()
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();

            return View(students);
        }

        // GET: /Grades/Kardex/Details/5
        public async Task<IActionResult> Details(int studentId)
        {
            // 1. Obtener estudiante
            var student = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.Preenrollments)
                .FirstOrDefaultAsync(u => u.UserId == studentId);

            if (student == null) return NotFound();

            // 2. Obtener TODAS las inscripciones (historial completo)
            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                    .ThenInclude(g => g.GradeLevel)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

            // 3. Agrupar por nivel (un alumno pudo cambiar de grupo dentro del mismo nivel)
            var semesterData = enrollments
                .GroupBy(e => e.Group.GradeLevelId)
                .Select(g => new
                {
                    GradeLevel = g.First().Group.GradeLevel,
                    GroupName = g.OrderByDescending(e => e.EnrolledAt).First().Group.Name,
                    GroupId = g.OrderByDescending(e => e.EnrolledAt).First().GroupId
                })
                .OrderBy(s => s.GradeLevel.StartDate)
                .ToList();

            // 4. Construir semestres
            var semesters = new List<KardexSemesterViewModel>();

            foreach (var sem in semesterData)
            {
                // Materias del nivel
                var subjects = await _context.grades_Subjects
                    .Include(s => s.Units)
                    .Where(s => s.GradeLevelId == sem.GradeLevel.GradeLevelId)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                // Calificaciones por unidad
                var unitGrades = await _context.grades_Grades
                    .Include(g => g.Recoveries)
                    .Include(g => g.SubjectUnit)
                    .Where(g => g.StudentId == studentId && g.GroupId == sem.GroupId)
                    .ToListAsync();

                // Calificaciones finales
                var finalGrades = await _context.grades_FinalGrades
                    .Include(f => f.ExtraordinaryGrade)
                    .Where(f => f.StudentId == studentId && f.GroupId == sem.GroupId)
                    .ToDictionaryAsync(f => f.SubjectId);

                // Construir materias
                var subjectVMs = subjects.Select(subject =>
                {
                    var subjectUnitGrades = unitGrades
                        .Where(g => g.SubjectUnit.SubjectId == subject.SubjectId)
                        .ToList();

                    var units = subject.Units
                        .OrderBy(u => u.UnitNumber)
                        .Select(u =>
                        {
                            var grade = subjectUnitGrades
                                .FirstOrDefault(g => g.SubjectUnitId == u.UnitId);
                            return new KardexUnitGradeViewModel
                            {
                                UnitNumber = u.UnitNumber,
                                Grade = grade?.Value,
                                Recovery = grade?.Recoveries?.FirstOrDefault()?.Value
                            };
                        }).ToList();

                    finalGrades.TryGetValue(subject.SubjectId, out var final_);

                    return new KardexSubjectViewModel
                    {
                        SubjectName = subject.Name,
                        Units = units,
                        FinalGrade = final_?.Value,
                        Passed = final_?.Passed ?? false,
                        HasExtraordinary = final_?.ExtraordinaryGrade != null,
                        ExtraordinaryGrade = final_?.ExtraordinaryGrade?.Value
                    };
                }).ToList();

                var withFinal = subjectVMs.Where(s => s.FinalGrade.HasValue).ToList();

                semesters.Add(new KardexSemesterViewModel
                {
                    GradeLevelId = sem.GradeLevel.GradeLevelId,
                    GradeLevelName = sem.GradeLevel.Name,
                    GroupName = sem.GroupName,
                    StartDate = sem.GradeLevel.StartDate,
                    EndDate = sem.GradeLevel.EndDate,
                    IsOpen = sem.GradeLevel.IsOpen,
                    Subjects = subjectVMs,
                    PromedioSemestre = withFinal.Any()
                        ? withFinal.Average(s => s.FinalGrade!.Value)
                        : 0
                });
            }

            // 5. Estadísticas generales
            var allSubjects = semesters.SelectMany(s => s.Subjects).ToList();
            var allWithFinal = allSubjects.Where(s => s.FinalGrade.HasValue).ToList();

            var matricula = student.Preenrollments?
                .Where(p => p.Matricula != null)
                .OrderByDescending(p => p.CreateStat)
                .Select(p => p.Matricula)
                .FirstOrDefault() ?? "S/N";

            var viewModel = new KardexViewModel
            {
                StudentId = student.UserId,
                StudentName = $"{student.Person.FirstName} {student.Person.LastNamePaternal} {student.Person.LastNameMaternal}",
                Matricula = matricula,
                Email = student.Email,
                Semesters = semesters,
                PromedioGeneral = allWithFinal.Any()
                    ? allWithFinal.Average(s => s.FinalGrade!.Value)
                    : 0,
                TotalMaterias = allSubjects.Count,
                MateriasAprobadas = allSubjects.Count(s => s.Passed && !s.HasExtraordinary),
                MateriasReprobadas = allSubjects.Count(s => !s.Passed),
                MateriasConExtraordinario = allSubjects.Count(s => s.HasExtraordinary)
            };

            return View(viewModel);
        }
    }
}