using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.SocialService.Controllers
{
    [Area("SocialService")]
    public class StudentController : Controller
    {
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
