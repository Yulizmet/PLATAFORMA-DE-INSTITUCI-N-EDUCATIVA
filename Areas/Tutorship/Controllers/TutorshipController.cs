using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Tutorship.Controllers
{
    [Area("Gestion")]
    public class TutorshipController : Controller
    {
        private readonly AppDbContext _context;

        private readonly int _simulatedRoleId = 2; 
        private readonly int _simulatedUserId = 8; 

        public TutorshipController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult AccesoDenegado()
        {
            return Content("No tienes permiso para ver esta pantalla. Tu rol actual es: " + _simulatedRoleId);
        }

        public IActionResult Controlador()
        {
            ViewBag.RoleId = _simulatedRoleId;
            return View("~/Areas/Tutorship/Views/Controlador.cshtml");
        }



        public async Task<IActionResult> EntrevistaInicial()
        {
            if (_simulatedRoleId != 1) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;

            // Verificamos si el alumno ya llenó la entrevista
            var existeEntrevista = await _context.TutorshipInterviews
                .AnyAsync(e => e.StudentId == _simulatedUserId);

            if (existeEntrevista)
            {
                // Si ya la llenó, lo mandamos a ver su detalle directamente
                return RedirectToAction(nameof(DetalleEntrevista));
            }

            return View("~/Areas/Tutorship/Views/EntrevistaInicial.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarEntrevista(tutorship_interview modelo)
        {
            if (_simulatedRoleId != 1) return RedirectToAction(nameof(AccesoDenegado));

            // 1. Datos de cabecera de la entrevista
            modelo.StudentId = _simulatedUserId;
            modelo.DateCompleted = DateTime.Now;
            modelo.Status = "Completada";
            modelo.FilePath = "N/A"; // Cambiar si después implementas la subida de foto/PDF real

            // 2. Entity Framework detecta automáticamente la lista "modelo.Answers" 
            // que viene del formulario HTML y las guarda en la tabla tutorship_interview_answer
            _context.TutorshipInterviews.Add(modelo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(DetalleEntrevista));
        }

        // Método que usa el ALUMNO para ver su PROPIA entrevista
        public async Task<IActionResult> DetalleEntrevista()
        {
            if (_simulatedRoleId != 1) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers) // Trae las respuestas de la tabla hija
                .Include(e => e.Student)
                .ThenInclude(s => s.Person)
                .FirstOrDefaultAsync(e => e.StudentId == _simulatedUserId);

            return View("~/Areas/Tutorship/Views/DetalleEntrevista.cshtml", entrevista);
        }

        // ==========================================
        // VISTAS Y ACCIONES DEL MAESTRO (Rol 2)
        // ==========================================

        // Método que usa el MAESTRO para ver la entrevista de UN ALUMNO
        public async Task<IActionResult> VerEntrevistaAlumno(int id)
        {
            if (_simulatedRoleId != 2) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers) // Trae las respuestas de la tabla hija
                .Include(e => e.Student)
                .ThenInclude(s => s.Person)
                .FirstOrDefaultAsync(e => e.StudentId == id);

            if (entrevista == null)
            {
                TempData["Mensaje"] = "Este alumno aún no ha llenado su entrevista inicial.";
                return RedirectToAction(nameof(ListaDeAlumnos));
            }

            // Usamos la misma vista que ve el alumno, pero con los datos del alumno seleccionado
            return View("~/Areas/Tutorship/Views/DetalleEntrevista.cshtml", entrevista);
        }

        public async Task<IActionResult> ListaDeAlumnos()
        {
            if (_simulatedRoleId != 2) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;

            var listaAlumnos = await _context.Users
                .Include(u => u.Person)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 1))
                .ToListAsync();

            return View("~/Areas/Tutorship/Views/ListaDeAlumnos.cshtml", listaAlumnos);
        }

        public IActionResult Asistencia()
        {
            if (_simulatedRoleId != 2) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;
            return View("~/Areas/Tutorship/Views/Asistencia.cshtml");
        }

        public IActionResult Seguimiento()
        {
            if (_simulatedRoleId != 2) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;
            return View("~/Areas/Tutorship/Views/Seguimiento.cshtml");
        }
    }
}