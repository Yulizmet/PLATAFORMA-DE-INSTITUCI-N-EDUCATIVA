using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.SocialService.Controllers
{
    [Area("SocialService")]
    public class StudentController : Controller
    {
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
            return View();
        }

        // Crear nueva bitácora
        public IActionResult CrearBitacora()
        {
            return View();
        }

        // Ver horas de prácticas y servicio social
        public IActionResult Horas()
        {
            return View();
        }
    }
}
