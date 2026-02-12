using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SchoolManager.Pages.Estadisticas
{
    public class IndexModel : PageModel
    {
        // Añadidos Genero y Semestre
        public record Student(int Id, string Nombre, string Curso, string Estado, double Nota, DateTime FechaInscripcion, string Genero, int Semestre);

        // Nuevo record para empleados con ActividadesHoy
        public record Employee(int Id, string Nombre, string Departamento, string Rol, string Genero, DateTime FechaContratacion, int ActividadesHoy);

        public List<Student> Students { get; private set; } = new();
        public List<Employee> Employees { get; private set; } = new();

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
            // Datos de ejemplo; reemplaza por acceso a BD cuando lo tengas
            Students = new List<Student>
            {
                new Student(1, "Ana Pérez", "Matemáticas", "Cursando", 0, DateTime.Parse("2025-09-01"), "F", 3),
                new Student(2, "Juan López", "Física", "Aprobado", 8.5, DateTime.Parse("2024-02-15"), "M", 6),
                new Student(3, "María García", "Química", "Reprobado", 4.0, DateTime.Parse("2023-11-10"), "F", 2),
                new Student(4, "Luis Torres", "Matemáticas", "Cursando", 0, DateTime.Parse("2025-09-10"), "M", 1),
                new Student(5, "Carla Ruiz", "Física", "Inscrito", 0, DateTime.Parse("2026-01-05"), "F", 1),
                new Student(6, "Pedro Gómez", "Química", "Aprobado", 9.2, DateTime.Parse("2024-06-20"), "M", 8),
                new Student(7, "Sofía Castillo", "Matemáticas", "Reprobado", 3.8, DateTime.Parse("2023-12-01"), "F", 4)
            };

            // Ejemplos de empleados con ActividadesHoy
            Employees = new List<Employee>
            {
                new Employee(1, "Laura Méndez", "Enfermería", "Enfermera", "F", DateTime.Parse("2020-05-01"), 12),
                new Employee(2, "Carlos Ramírez", "Administración", "Auxiliar", "M", DateTime.Parse("2019-08-15"), 5),
                new Employee(3, "Elena Soto", "Enfermería", "Enfermera Jefe", "F", DateTime.Parse("2018-03-10"), 18),
                new Employee(4, "Mario Ruiz", "Mantenimiento", "Técnico", "M", DateTime.Parse("2022-11-01"), 7)
            };

            JsonStudents = JsonSerializer.Serialize(Students);
            JsonEmployees = JsonSerializer.Serialize(Employees);
        }
    }
}