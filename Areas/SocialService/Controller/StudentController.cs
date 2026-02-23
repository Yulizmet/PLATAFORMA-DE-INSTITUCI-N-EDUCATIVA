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
                // TODO: Obtener el ID del estudiante actual desde la sesión/autenticación
                int currentStudentId = 1; // Valor temporal

                // Verificar si ya existe una bitácora para esta semana
                var existingLog = _context.SocialServiceLogs
                    .FirstOrDefault(log => log.StudentId == currentStudentId && log.Week == vm.Week);

                if (existingLog != null)
                {
                    // Si ya existe una bitácora para esta semana, mostrar error
                    TempData["Error"] = $"Ya tienes una bitácora registrada para la {vm.Week}. No puedes crear más de una bitácora por semana.";
                    return View(vm);
                }

                // Crear un nuevo objeto del modelo que representa la tabla social_service_log
                var log = new social_service_log
                {
                    // ID del estudiante (temporal, luego se conectará con el usuario logueado)
                    StudentId = currentStudentId,

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

                // Mensaje de éxito
                TempData["Success"] = "Bitácora registrada exitosamente.";

                // Redirige a la vista de bitácoras después de guardar
                return RedirectToAction("Bitacoras");
            }

            // Si hay error, vuelve a mostrar el formulario con los datos
            return View(vm);
        }

        // Ver horas de prácticas y servicio social
        public IActionResult Horas()
        {
            // TODO: Obtener el ID del estudiante actual desde la sesión/autenticación
            int currentStudentId = 1; // Valor temporal

            // Calcular horas totales aprobadas
            var approvedLogs = _context.SocialServiceLogs
                .Where(log => log.StudentId == currentStudentId && log.IsApproved)
                .ToList();

            int totalHoursPracticas = approvedLogs.Sum(log => log.ApprovedHoursPracticas);
            int totalHoursServicioSocial = approvedLogs.Sum(log => log.ApprovedHoursServicioSocial);

            // Horas requeridas (podrían venir de configuración)
            int requiredHoursPracticas = 480;
            int requiredHoursServicioSocial = 480;

            // Calcular porcentajes
            ViewBag.TotalHoursPracticas = totalHoursPracticas;
            ViewBag.RequiredHoursPracticas = requiredHoursPracticas;
            ViewBag.RemainingHoursPracticas = Math.Max(0, requiredHoursPracticas - totalHoursPracticas);
            ViewBag.PercentagePracticas = requiredHoursPracticas > 0 
                ? (int)((double)totalHoursPracticas / requiredHoursPracticas * 100) 
                : 0;

            ViewBag.TotalHoursServicioSocial = totalHoursServicioSocial;
            ViewBag.RequiredHoursServicioSocial = requiredHoursServicioSocial;
            ViewBag.RemainingHoursServicioSocial = Math.Max(0, requiredHoursServicioSocial - totalHoursServicioSocial);
            ViewBag.PercentageServicioSocial = requiredHoursServicioSocial > 0 
                ? (int)((double)totalHoursServicioSocial / requiredHoursServicioSocial * 100) 
                : 0;

            return View();
        }
    }
}