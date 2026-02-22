using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.SocialService.Controllers
{
    [Area("SocialService")]
    public class TeacherController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Alumnos()
        {
            return View();
        }

        // 👉 VER BITÁCORAS DE UN ALUMNO ESPECÍFICO
        public IActionResult RevisarBitacorasAlumno(int id)
        {
            // Simulación de alumno seleccionado
            ViewBag.AlumnoId = id;
            ViewBag.NombreAlumno = "Juan Pérez";

            return View();
        }


        public IActionResult AsignarHoras()
        {
            return View();
        }

        public IActionResult Asistencia()
        {
            return View();
        }
    }
}
