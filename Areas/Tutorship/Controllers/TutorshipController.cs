using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.Tutorship.Controllers
{
    [Area("Gestion")]
    ////localhost:7207/Gestion/Tutorias/Seguimiento
    //[Route("PanelTutorias")]
    //localhost/PanelTutorias/Seguimiento
    public class TutorshipController : Controller
    {
        public IActionResult Asistencia()
        {
            return View("~/Areas/Tutorship/Views/Asistencia.cshtml");
        }

        public IActionResult EntrevistaInicial()
        {
            return View("~/Areas/Tutorship/Views/EntrevistaInicial.cshtml");
        }

        public IActionResult DetalleEntrevista()
        {
            return View("~/Areas/Tutorship/Views/DetalleEntrevista.cshtml");
        }

        public IActionResult Seguimiento()
        {
            return View("~/Areas/Tutorship/Views/Seguimiento.cshtml");
        }

        public IActionResult Controlador()
        {
            return View("~/Areas/Tutorship/Views/Controlador.cshtml");
        }
        public IActionResult ListaDeAlumnos()
        {
            return View("~/Areas/Tutorship/Views/ListaDeAlumnos.cshtml");
        }
    }
}
