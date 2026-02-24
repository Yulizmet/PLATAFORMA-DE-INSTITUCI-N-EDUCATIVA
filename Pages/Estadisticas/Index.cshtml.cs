using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Pages.Estadisticas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

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
            Students = _context.Students.ToList();
            Employees = _context.Employees.ToList();

            JsonStudents = JsonSerializer.Serialize(Students);
            JsonEmployees = JsonSerializer.Serialize(Employees);
        }
    }
}
