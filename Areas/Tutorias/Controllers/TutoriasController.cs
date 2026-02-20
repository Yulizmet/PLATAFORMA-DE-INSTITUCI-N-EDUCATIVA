using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.Tutorias.Controllers
{
    [Area("Gestion")]
    ////localhost:7207/Gestion/Tutorias/Seguimiento
    //[Route("PanelTutorias")]
    //localhost/PanelTutorias/Seguimiento
    public class TutoriasController : Controller
    {
        public IActionResult Asistencia()
        {
            return View("~/Areas/Tutorias/Views/Asistencia.cshtml");
        }

        public IActionResult EntrevistaInicial()
        {
            return View("~/Areas/Tutorias/Views/EntrevistaInicial.cshtml");
        }

        public IActionResult DetalleEntrevista()
        {
            return View("~/Areas/Tutorias/Views/DetalleEntrevista.cshtml");
        }

        public IActionResult Seguimiento()
        {
            return View("~/Areas/Tutorias/Views/Seguimiento.cshtml");
        }
    }
}
