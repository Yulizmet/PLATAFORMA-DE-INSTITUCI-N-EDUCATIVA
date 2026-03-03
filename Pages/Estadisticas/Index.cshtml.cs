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
        // Propiedades para almacenar los datos de estudiantes y empleados

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
                var raw = (from user in _context.Users
                           join person in _context.Persons on user.PersonId equals person.PersonId into personJoin
                           from person in personJoin.DefaultIfEmpty()
                           join userRole in _context.UserRoles on user.UserId equals userRole.UserId into roleJoin
                           from userRole in roleJoin.DefaultIfEmpty()
                           join role in _context.Roles on userRole.RoleId equals role.RoleId into roleDetailJoin
                           from role in roleDetailJoin.DefaultIfEmpty()
                           join finalGrade in _context.grades_FinalGrades on user.UserId equals finalGrade.StudentId into gradeJoin
                           from finalGrade in gradeJoin.DefaultIfEmpty()
                           join subject in _context.grades_Subjects on (finalGrade != null ? finalGrade.SubjectId : -1) equals subject.SubjectId into subjectJoin
                           from subject in subjectJoin.DefaultIfEmpty()
                           where role != null && role.Name == "Student"
                           select new
                           {
                               user.UserId,
                               FirstName = (string?)person.FirstName,
                               LastName = (string?)person.LastNamePaternal,
                               Genero = (string?)person.Gender,
                               Curso = (string?)subject.Name,
                               Semestre = finalGrade != null ? finalGrade.GroupId : 0,
                               Nota = finalGrade != null ? (double)finalGrade.Value : 0.0,
                               FechaInscripcion = user.CreatedDate,
                               HasGrade = finalGrade != null,
                               Passed = finalGrade != null && finalGrade.Passed
                           })
                           .Distinct()
                           .ToList();

                Students = raw
                    .GroupBy(x => x.UserId)
                    .Select(g =>
                    {
                        var s = g.First();
                        var nombre = $"{s.FirstName ?? string.Empty} {s.LastName ?? string.Empty}".Trim();
                        return new StudentStatisticsVM
                        {
                            Id = s.UserId,
                            Nombre = string.IsNullOrWhiteSpace(nombre) ? "Sin nombre" : nombre,
                            Genero = string.IsNullOrWhiteSpace(s.Genero) ? "N/A" : s.Genero,
                            Curso = string.IsNullOrWhiteSpace(s.Curso) ? "Sin asignar" : s.Curso,
                            Semestre = s.Semestre,
                            Nota = s.Nota,
                            FechaInscripcion = s.FechaInscripcion,
                            Estado = !s.HasGrade ? "Inscrito"
                                             : s.Passed ? "Aprobado"
                                             : "Reprobado"
                        };
                    })
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
                var raw = (from user in _context.Users
                           join person in _context.Persons on user.PersonId equals person.PersonId into personJoin
                           from person in personJoin.DefaultIfEmpty()
                           join userRole in _context.UserRoles on user.UserId equals userRole.UserId into roleJoin
                           from userRole in roleJoin.DefaultIfEmpty()
                           join role in _context.Roles on userRole.RoleId equals role.RoleId into roleDetailJoin
                           from role in roleDetailJoin.DefaultIfEmpty()
                           where role != null && role.Name == "Teacher"
                           select new
                           {
                               user.UserId,
                               FirstName = (string?)person.FirstName,
                               LastName = (string?)person.LastNamePaternal,
                               Genero = (string?)person.Gender,
                               Departamento = (string?)role.Name,
                               Rol = (string?)role.Name,
                               FechaContratacion = (DateTime?)userRole.CreatedDate
                           })
                           .Distinct()
                           .ToList();

                Employees = raw
                    .GroupBy(x => x.UserId)
                    .Select(g =>
                    {
                        var e = g.First();
                        var nombre = $"{e.FirstName ?? string.Empty} {e.LastName ?? string.Empty}".Trim();
                        return new EmployeeStatisticsVM
                        {
                            Id = e.UserId,
                            Nombre = string.IsNullOrWhiteSpace(nombre) ? "Sin nombre" : nombre,
                            Genero = string.IsNullOrWhiteSpace(e.Genero) ? "N/A" : e.Genero,
                            Departamento = string.IsNullOrWhiteSpace(e.Departamento) ? "Sin asignar" : e.Departamento,
                            Rol = string.IsNullOrWhiteSpace(e.Rol) ? "Sin rol" : e.Rol,
                            ActividadesHoy = 0,
                            FechaContratacion = e.FechaContratacion ?? DateTime.Now
                        };
                    })
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
