using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SchoolManager.Pages.Estadisticas
{
    public class IndexModel : PageModel
    {
        public record Student(int Id, string Nombre, string Curso, string Estado, double Nota, DateTime FechaInscripcion);

        public List<Student> Students { get; private set; } = new();
        public string JsonStudents { get; private set; } = "[]";

        public int Total => Students.Count;
        public int CountInscrito => Students.Count(s => s.Estado == "Inscrito");
        public int CountCursando => Students.Count(s => s.Estado == "Cursando");
        public int CountAprobado => Students.Count(s => s.Estado == "Aprobado");
        public int CountReprobado => Students.Count(s => s.Estado == "Reprobado");

        public void OnGet()
        {
            // Datos de ejemplo; reemplaza por acceso a BD cuando lo tengas
            Students = new List<Student>
            {
                new Student(1, "Ana Pérez", "Matemáticas", "Cursando", 0, DateTime.Parse("2025-09-01")),
                new Student(2, "Juan López", "Física", "Aprobado", 8.5, DateTime.Parse("2024-02-15")),
                new Student(3, "María García", "Química", "Reprobado", 4.0, DateTime.Parse("2023-11-10")),
                new Student(4, "Luis Torres", "Matemáticas", "Cursando", 0, DateTime.Parse("2025-09-10")),
                new Student(5, "Carla Ruiz", "Física", "Inscrito", 0, DateTime.Parse("2026-01-05")),
                new Student(6, "Pedro Gómez", "Química", "Aprobado", 9.2, DateTime.Parse("2024-06-20")),
                new Student(7, "Sofía Castillo", "Matemáticas", "Reprobado", 3.8, DateTime.Parse("2023-12-01"))
            };

            JsonStudents = JsonSerializer.Serialize(Students);
        }
    }
}