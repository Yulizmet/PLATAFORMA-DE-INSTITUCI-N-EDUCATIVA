using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.MainScreen.ViewModels;
using SchoolManager.Data;

namespace SchoolManager.Areas.MainScreen.Controllers
{
    [Area("MainScreen")]
    [Route("MainScreen/[controller]/[action]")]
    public class MainScreenController : Controller
    {
        private readonly AppDbContext _context;

        public MainScreenController(AppDbContext context) {
            _context = context;
        }
        [AllowAnonymous]
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
            // Verificar si el usuario es estudiante y si está asignado a un maestro de servicio social
            if (User.IsInRole("Student"))
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    int userId = int.Parse(userIdClaim);
                    bool isAssigned = _context.SocialServiceAssignments
                        .Any(a => a.StudentId == userId && a.IsActive);

                    ViewBag.IsStudentAssignedToSocialService = isAssigned;
                }
                else
                {
                    ViewBag.IsStudentAssignedToSocialService = false;
                }
            }
            else
            {
                // Maestros y administradores siempre tienen acceso
                ViewBag.IsStudentAssignedToSocialService = true;
            }

            return View();
        }

        [Route("/PanelAdministrativo")]
        public async Task<IActionResult> PanelAdministrativo()
        {
            var asignaciones = await _context.grades_TeacherSubjects
                .Include(ts => ts.Teacher).ThenInclude(t => t.Person)
                .Include(ts => ts.Subject).ThenInclude(s => s.GradeLevel)
                .Select(ts => new AsignacionResumenViewModel
                {
                    TeacherSubjectId = ts.TeacherSubjectId,
                    TeacherName = ts.Teacher.Person.FirstName + " " + ts.Teacher.Person.LastNamePaternal,
                    SubjectName = ts.Subject.Name,
                    GradeLevelName = ts.Subject.GradeLevel.Name
                })
                .ToListAsync();

            var grupos = await _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .Select(g => new GrupoResumenViewModel
                {
                    GroupId = g.GroupId,
                    Name = g.Name,
                    GradeLevelName = g.GradeLevel.Name
                })
                .ToListAsync();

            var viewModel = new PanelAdministrativoViewModel
            {
                Asignaciones = asignaciones,
                Grupos = grupos
            };

            return View(viewModel);
        }


        [Route("/CalificacionesDocente")]
        public IActionResult CalificacionesDocente()
        {
            return View();
        }
        [Route("/CalificacionesEstudiante")]
        public IActionResult CalificacionesEstudiante()
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
        [AllowAnonymous]
        [Route("/Tramites")]
        public IActionResult Tramites()
        {
            return View();
        }
        [AllowAnonymous]
        [Route("/Foro")]
        public IActionResult Foro()
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