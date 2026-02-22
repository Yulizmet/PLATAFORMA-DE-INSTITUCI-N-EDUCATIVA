using Microsoft.AspNetCore.Mvc;

namespace PI.Controllers
{
    public class AlumnoController : Controller
    {
        // Dashboard del alumno
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
