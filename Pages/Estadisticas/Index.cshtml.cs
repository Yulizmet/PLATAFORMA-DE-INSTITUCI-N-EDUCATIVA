using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SchoolManager.Data;
using SchoolManager.ViewModels;

namespace SchoolManager.Pages.Estadisticas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<StudentStatisticsVM> Students { get; private set; } = new();
        public List<EmployeeStatisticsVM> Employees { get; private set; } = new();

        public string JsonStudents { get; private set; } = "[]";
        public string JsonEmployees { get; private set; } = "[]";

        public int Total => Students.Count;
        public int CountInscrito => Students.Count(s => s.Estado == "Inscrito");
        public int CountCursando => Students.Count(s => s.Estado == "Cursando");
        public int CountAprobado => Students.Count(s => s.Estado == "Aprobado");
        public int CountReprobado => Students.Count(s => s.Estado == "Reprobado");

        public int EmployeesTotal => Employees.Count;

        public void OnGet()
        {
            LoadStudents();
            LoadEmployees();

            JsonStudents = JsonSerializer.Serialize(Students);
            JsonEmployees = JsonSerializer.Serialize(Employees);
        }

        private void LoadStudents()
        {
            try
            {
                var students = (from user in _context.Users
                                join person in _context.Persons on user.PersonId equals person.Id into personJoin
                                from person in personJoin.DefaultIfEmpty()
                                join userRole in _context.UserRoles on user.Id equals userRole.UserId into roleJoin
                                from userRole in roleJoin.DefaultIfEmpty()
                                join role in _context.Roles on userRole.RoleId equals role.Id into roleDetailJoin
                                from role in roleDetailJoin.DefaultIfEmpty()
                                join finalGrade in _context.GradeFinalGrades on user.Id equals finalGrade.StudentId into gradeJoin
                                from finalGrade in gradeJoin.DefaultIfEmpty()
                                join subject in _context.GradeSubjects on finalGrade != null ? finalGrade.SubjectId : -1 equals subject.Id into subjectJoin
                                from subject in subjectJoin.DefaultIfEmpty()
                                where role.Name == "Student"
                                select new
                                {
                                    user.Id,
                                    Nombre = (person.FirstName + " " + person.LastNamePaternal).Trim(),
                                    Genero = person.Gender ?? "N/A",
                                    Curso = subject.Name ?? "Sin asignar",
                                    Semestre = finalGrade != null ? finalGrade.GroupId : 0,
                                    Nota = finalGrade != null ? finalGrade.Value : 0.0,
                                    FechaInscripcion = user.CreatedDate,
                                    Passed = finalGrade != null ? finalGrade.Passed : false
                                })
                                .Distinct()
                                .ToList();

                Students = students.Select(s => new StudentStatisticsVM
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Genero = s.Genero,
                    Curso = s.Curso,
                    Semestre = s.Semestre,
                    Nota = s.Nota,
                    FechaInscripcion = s.FechaInscripcion,
                    Estado = s.Passed ? "Aprobado" : "Reprobado"
                })
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando estudiantes: {ex.Message}");
                Students = new List<StudentStatisticsVM>();
            }
        }

        private void LoadEmployees()
        {
            try
            {
                var employees = (from user in _context.Users
                                 join person in _context.Persons on user.PersonId equals person.Id into personJoin
                                 from person in personJoin.DefaultIfEmpty()
                                 join userRole in _context.UserRoles on user.Id equals userRole.UserId into roleJoin
                                 from userRole in roleJoin.DefaultIfEmpty()
                                 join role in _context.Roles on userRole.RoleId equals role.Id into roleDetailJoin
                                 from role in roleDetailJoin.DefaultIfEmpty()
                                 where role.Name == "Teacher"
                                 select new
                                 {
                                     user.Id,
                                     Nombre = (person.FirstName + " " + person.LastNamePaternal).Trim(),
                                     Genero = person.Gender ?? "N/A",
                                     Departamento = role.Name,
                                     Rol = role.Name,
                                     FechaContratacion = userRole.CreatedDate
                                 })
                                 .Distinct()
                                 .ToList();

                Employees = employees.Select(e => new EmployeeStatisticsVM
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Genero = e.Genero,
                    Departamento = e.Departamento,
                    Rol = e.Rol,
                    ActividadesHoy = 0,
                    FechaContratacion = e.FechaContratacion
                })
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando empleados: {ex.Message}");
                Employees = new List<EmployeeStatisticsVM>();
            }
        }
    }
}
