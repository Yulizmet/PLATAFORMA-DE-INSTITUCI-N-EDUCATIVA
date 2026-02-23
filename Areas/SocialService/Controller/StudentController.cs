using Microsoft.AspNetCore.Mvc;

// permite acceder a la base de datos mediante Entity Framework
using SchoolManager.Data;

// permite usar el ViewModel que contiene los datos del formulario
using SchoolManager.Areas.SocialService.ViewModels;

// permite usar el modelo de la tabla social_service_log
using SchoolManager.Models;

namespace SchoolManager.Areas.SocialService.Controllers
{
    [Area("SocialService")]
    public class StudentController : Controller
    {
        // variable privada para acceder a la base de datos
        private readonly AppDbContext _context;

        // constructor que recibe el contexto de la base de datos
        // Esto permite guardar y leer información desde SQL Server
        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        // Al acceder a /SocialService/Student redirige al dashboard del estudiante
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // Dashboard del estudiante
        public IActionResult Dashboard()
        {
            return View();
        }

        // Ver bitácoras anteriores
        public IActionResult Bitacoras()
        {
            // Obtener las bitácoras del estudiante (por ahora StudentId = 1)
            var bitacoras = _context.SocialServiceLogs
                .Where(x => x.StudentId == 1)
                .OrderByDescending(x => x.Week)
                .ToList();

            return View(bitacoras);
        }

        // Crear nueva bitácora (GET)
        // Este método solo muestra el formulario vacío
        public IActionResult CrearBitacora()
        {
            return View();
        }

        // Crear nueva bitácora (POST)
        // Este método se ejecuta cuando el estudiante presiona el botón "Guardar Bitácora"
        [HttpPost]
        public IActionResult CrearBitacora(BitacoraViewModel vm)
        {
            // Verifica que los datos del formulario sean válidos
            if (ModelState.IsValid)
            {
                // Crear un nuevo objeto del modelo que representa la tabla social_service_log
                var log = new social_service_log
                {
                    // ID del estudiante (temporal, luego se conectará con el usuario logueado)
                    StudentId = 1,

                    // Datos provenientes del formulario
                    Week = vm.Week,
                    Activities = vm.Activities,
                    HoursPracticas = vm.HoursPracticas,
                    HoursServicioSocial = vm.HoursServicioSocial,
                    Observations = vm.Observations,

                    // Fecha y hora actual
                    CreatedAt = DateTime.Now
                };

                // Agrega el registro al contexto
                _context.SocialServiceLogs.Add(log);

                // Guarda los cambios en la base de datos
                _context.SaveChanges();

                // Redirige a la vista de bitácoras después de guardar
                return RedirectToAction("Bitacoras");
            }

            // Si hay error, vuelve a mostrar el formulario con los datos
            return View(vm);
        }

        // Ver horas de prácticas y servicio social
        public IActionResult Horas()
        {
            return View();
        }
    }
}