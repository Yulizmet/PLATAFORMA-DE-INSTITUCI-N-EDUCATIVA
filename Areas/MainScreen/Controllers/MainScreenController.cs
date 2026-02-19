using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.MainScreen.Controllers
{
    [Area("MainScreen")]
    public class MainScreenController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Admin()
        {
            return View();
        }

        public IActionResult Estudiantes()
        {
            return View();
        }

        public IActionResult Docentes()
        {
            return View();
        }

        public IActionResult SistemaEscolar()
        {
            return View();
        }

        public IActionResult Extension()
        {
            return View();
        }

        public IActionResult Tramites()
        {
            return View();
        }

        public IActionResult ProcesosInscripcion()
        {
            return View();
        }


    }
}
