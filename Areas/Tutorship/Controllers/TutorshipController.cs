using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;

namespace SchoolManager.Areas.Tutorship.Controllers
{
    [Area("Gestion")]
    ////localhost:7207/Gestion/Tutorias/Seguimiento
    //[Route("PanelTutorias")]
    //localhost/PanelTutorias/Seguimiento
    public class TutorshipController : Controller
    {

        private readonly AppDbContext _context;

        public TutorshipController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Asistencia()
        {
            return View("~/Areas/Tutorship/Views/Asistencia.cshtml");
        }

        public async Task<IActionResult> EntrevistaInicial()
        {
            int usuarioIdDePrueba = 11;

            var usuario = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == usuarioIdDePrueba);

            if (usuario == null)
            {
                return NotFound("El usuario de prueba no fue encontrado.");
            }


            var matriculaAlumno = await _context.PreenrollmentGenerals
        .Where(p => p.UserId == usuarioIdDePrueba)
        .Select(p => p.Matricula)
        .FirstOrDefaultAsync();

            ViewBag.Matricula = matriculaAlumno ?? "Sin asignar";

            return View("~/Areas/Tutorship/Views/EntrevistaInicial.cshtml", usuario);
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
        public async Task<IActionResult> ListaDeAlumnos()
        {
            var listaAlumnos = await _context.Users
        .Include(u => u.Person)
        .ToListAsync();

            return View("~/Areas/Tutorship/Views/ListaDeAlumnos.cshtml", listaAlumnos);
        }
    }
}
