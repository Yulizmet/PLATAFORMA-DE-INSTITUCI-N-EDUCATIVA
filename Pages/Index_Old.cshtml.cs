using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SchoolManager.Pages
{
    public class IndexModel : PageModel
    {
        public record Student(int Id, string Nombre, string Curso, string Estado, double Nota, DateTime FechaInscripcion);

        // Propiedad JsonStudents movida a la parte superior
        public string JsonStudents => JsonSerializer.Serialize(Students);

        // Asegúrate de que esta propiedad esté definida y sea pública
        public List<StudentViewModel> Students { get; set; }

        // Inicialización de Students en la declaración de la propiedad
        // public List<Student> Students { get; set; } = new List<Student>();

        public int Total { get; set; }
        public int CountInscrito { get; set; }
        public int CountCursando { get; set; }
        public int CountAprobado { get; set; }
        public int CountReprobado { get; set; }

        public void OnGet()
        {
            // Datos de ejemplo — reemplaza por acceso a BD cuando lo tengas
            Students = new List<StudentViewModel>
            {
                new StudentViewModel { Id = 1, Nombre = "Ana Pérez", Curso = "Matemáticas", Estado = "Cursando", Nota = 0, FechaInscripcion = DateTime.Parse("2025-09-01") },
                new StudentViewModel { Id = 2, Nombre = "Juan López", Curso = "Física", Estado = "Aprobado", Nota = 8.5, FechaInscripcion = DateTime.Parse("2024-02-15") },
                new StudentViewModel { Id = 3, Nombre = "María García", Curso = "Química", Estado = "Reprobado", Nota = 4.0, FechaInscripcion = DateTime.Parse("2023-11-10") },
                new StudentViewModel { Id = 4, Nombre = "Luis Torres", Curso = "Matemáticas", Estado = "Cursando", Nota = 0, FechaInscripcion = DateTime.Parse("2025-09-10") },
                new StudentViewModel { Id = 5, Nombre = "Carla Ruiz", Curso = "Física", Estado = "Inscrito", Nota = 0, FechaInscripcion = DateTime.Parse("2026-01-05") },
                new StudentViewModel { Id = 6, Nombre = "Pedro Gómez", Curso = "Química", Estado = "Aprobado", Nota = 9.2, FechaInscripcion = DateTime.Parse("2024-06-20") },
                new StudentViewModel { Id = 7, Nombre = "Sofía Castillo", Curso = "Matemáticas", Estado = "Reprobado", Nota = 3.8, FechaInscripcion = DateTime.Parse("2023-12-01") }
            };

            // Cálculo de estadísticas
            Total = Students.Count;
            CountInscrito = Students.Count(s => s.Estado == "Inscrito");
            CountCursando = Students.Count(s => s.Estado == "Cursando");
            CountAprobado = Students.Count(s => s.Estado == "Aprobado");
            CountReprobado = Students.Count(s => s.Estado == "Reprobado");
        }
    }

    // Asegúrate de tener este ViewModel o uno equivalente
    public class StudentViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Curso { get; set; }
        public string Estado { get; set; }
        public double Nota { get; set; }
        public System.DateTime FechaInscripcion { get; set; }
    }
}