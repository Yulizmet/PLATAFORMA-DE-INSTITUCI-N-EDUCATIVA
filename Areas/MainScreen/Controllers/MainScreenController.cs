using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.MainScreen.Controllers
{
    [Area("MainScreen")]
    [Route("MainScreen/[controller]/[action]")]
    public class MainScreenController : Controller
    {
        [Route("/", Order = -1)]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        [Route("/Estudiantes")]
        public IActionResult Estudiantes()
        {
            return View();
        }

        [Route("/Docentes")]
        public IActionResult Docentes()
        {
            return View();
        }

        [Route("/SistemaEscolar")]
        public IActionResult SistemaEscolar()
        {
            return View();
        }

        [Route("/ServicioSocial")]
        public IActionResult ServicioSocial()
        {
            return View();
        }

        [Route("/Extension")]
        public IActionResult Extension()
        {
            return View();
        }

        [Route("/Tramites")]
        public IActionResult Tramites()
        {
            return View();
        }

        [Route("/ProcesoInscripcion")]
        public IActionResult ProcesosInscripcion()
        {
            return View();
        }


    }
}