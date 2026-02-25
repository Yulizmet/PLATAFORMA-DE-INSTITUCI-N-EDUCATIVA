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

        // ==========================================
        // VARIABLES DE PRUEBA (Sistema de tu compañero)
        // Rol 1 = Alumno | Rol 2 = Maestro
        // ==========================================
        private readonly int _simulatedRoleId = 1; // Puesto en 1 para que puedas probar la Entrevista
        private readonly int _simulatedUserId = 11; // Puesto en 11 para usar tu alumno de prueba

        public TutorshipController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // SEGURIDAD
        // ==========================================
        public IActionResult AccesoDenegado()
        {
            return Content("No tienes permiso para ver esta pantalla. Tu rol actual es: " + _simulatedRoleId);
        }

        // ==========================================
        // VISTA PRINCIPAL (Auto-aprovisionamiento integrado)
        // ==========================================
        public async Task<IActionResult> Controlador()
        {
            ViewBag.RoleId = _simulatedRoleId; // De tu compañero

            // 1. AUTO-APROVISIONAMIENTO: Revisamos si ya existe la entrevista
            var entrevistaExistente = await _context.TutorshipInterviews.FirstOrDefaultAsync(e => e.StudentId == _simulatedUserId);

            if (entrevistaExistente == null)
            {
                var nuevaEntrevista = new tutorship_interview
                {
                    StudentId = _simulatedUserId,
                    Status = "Pendiente",
                    FilePath = "Sin archivo",
                    DateCompleted = DateTime.Now
                };
                _context.TutorshipInterviews.Add(nuevaEntrevista);
                await _context.SaveChangesAsync();
            }

            // 2. Buscamos el nombre del usuario para darle la bienvenida
            var usuario = await _context.Users.Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == _simulatedUserId);

            ViewBag.NombreUsuario = (usuario != null && usuario.Person != null) ? usuario.Person.FirstName : "Alumno";

            return View("~/Areas/Tutorship/Views/Controlador.cshtml");
        }

        // ==========================================
        // MÓDULOS DEL ALUMNO (Rol 1)
        // ==========================================
        public async Task<IActionResult> EntrevistaInicial()
        {
            // Seguridad de tu compañero
            if (_simulatedRoleId != 1) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;

            // 1. Traemos los datos básicos del alumno
            var usuario = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == _simulatedUserId);

            if (usuario == null) return NotFound("Usuario no encontrado.");

            // 2. Buscamos su matrícula en la tabla de preinscripciones
            var matriculaAlumno = await _context.PreenrollmentGenerals
                .Where(p => p.UserId == _simulatedUserId)
                .Select(p => p.Matricula)
                .FirstOrDefaultAsync();
            ViewBag.Matricula = matriculaAlumno ?? "Sin asignar";

            // 3. Traemos su entrevista generada y sus respuestas (para bloquear la vista si ya la llenó)
            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .FirstOrDefaultAsync(e => e.StudentId == _simulatedUserId);

            ViewBag.Entrevista = entrevista;

            return View("~/Areas/Tutorship/Views/EntrevistaInicial.cshtml", usuario);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarEntrevista(EntrevistaViewModel modelo)
        {
            // Seguridad de tu compañero
            if (_simulatedRoleId != 1) return RedirectToAction(nameof(AccesoDenegado));

            // 1. Buscamos el expediente
            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .FirstOrDefaultAsync(e => e.StudentId == modelo.UserId);

            if (entrevista == null) return NotFound();

            entrevista.Status = "Completada";
            entrevista.DateCompleted = DateTime.Now;
            _context.TutorshipInterviews.Update(entrevista);

            // 2. MODO UPDATE: Si ya había respuestas previas
            if (entrevista.Answers != null && entrevista.Answers.Any())
            {
                foreach (var respuesta in entrevista.Answers)
                {
                    if (respuesta.QuestionText.Contains("1. Estado Civil")) respuesta.AnswerText = modelo.EstadoCivil ?? "N/A";
                    else if (respuesta.QuestionText.Contains("2. ¿Tienes hijos?")) respuesta.AnswerText = modelo.TieneHijos ?? "N/A";
                    else if (respuesta.QuestionText.Contains("3. ¿Trabajas actualmente?")) respuesta.AnswerText = modelo.Trabaja ?? "N/A";
                    else if (respuesta.QuestionText.Contains("4. Medio de transporte")) respuesta.AnswerText = modelo.Transporte ?? "N/A";
                    else if (respuesta.QuestionText.Contains("5. ¿Cuántos hermanos")) respuesta.AnswerText = modelo.Hermanos ?? "N/A";
                    else if (respuesta.QuestionText.Contains("6. ¿Con quién vives")) respuesta.AnswerText = modelo.ViveCon ?? "N/A";
                    else if (respuesta.QuestionText.Contains("7. Describe brevemente")) respuesta.AnswerText = modelo.SituacionFamiliar ?? "N/A";
                    else if (respuesta.QuestionText.Contains("8. Promedio")) respuesta.AnswerText = modelo.PromedioPrepa ?? "N/A";
                    else if (respuesta.QuestionText.Contains("9. ¿Reprobaste")) respuesta.AnswerText = modelo.ReproboAnterior ?? "N/A";
                    else if (respuesta.QuestionText.Contains("10. ¿Cuentas con equipo")) respuesta.AnswerText = modelo.EquipoComputo ?? "N/A";
                    else if (respuesta.QuestionText.Contains("11. Calidad de tu Internet")) respuesta.AnswerText = modelo.Internet ?? "N/A";
                    else if (respuesta.QuestionText.Contains("12. ¿Por qué elegiste")) respuesta.AnswerText = modelo.MotivoCarrera ?? "N/A";
                }
                _context.TutorshipInterviewAnswers.UpdateRange(entrevista.Answers);
            }
            // 3. MODO INSERT: Si es la primera vez que la llena
            else
            {
                string catPersonal = "Aspectos Personales y Familiares";
                string catAcademico = "Antecedentes y Hábitos Académicos";

                var listaRespuestas = new List<tutorship_interview_answer>
                {
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catPersonal, QuestionText = "1. Estado Civil", AnswerText = modelo.EstadoCivil ?? "N/A" },
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catPersonal, QuestionText = "2. ¿Tienes hijos?", AnswerText = modelo.TieneHijos ?? "N/A" },
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catPersonal, QuestionText = "3. ¿Trabajas actualmente?", AnswerText = modelo.Trabaja ?? "N/A" },
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catPersonal, QuestionText = "4. Medio de transporte principal", AnswerText = modelo.Transporte ?? "N/A" },
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catPersonal, QuestionText = "5. ¿Cuántos hermanos tienes?", AnswerText = modelo.Hermanos ?? "N/A" },
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catPersonal, QuestionText = "6. ¿Con quién vives actualmente?", AnswerText = modelo.ViveCon ?? "N/A" },
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catPersonal, QuestionText = "7. Describe brevemente tu situación familiar actual", AnswerText = modelo.SituacionFamiliar ?? "N/A" },
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catAcademico, QuestionText = "8. Promedio de Bachillerato / Cuatrimestre anterior", AnswerText = modelo.PromedioPrepa ?? "N/A" },
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catAcademico, QuestionText = "9. ¿Reprobaste materias en el nivel anterior?", AnswerText = modelo.ReproboAnterior ?? "N/A" },
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catAcademico, QuestionText = "10. ¿Cuentas con equipo de cómputo propio?", AnswerText = modelo.EquipoComputo ?? "N/A" },
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catAcademico, QuestionText = "11. Calidad de tu Internet en casa", AnswerText = modelo.Internet ?? "N/A" },
                    new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catAcademico, QuestionText = "12. ¿Por qué elegiste esta carrera?", AnswerText = modelo.MotivoCarrera ?? "N/A" }
                };
                _context.TutorshipInterviewAnswers.AddRange(listaRespuestas);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("EntrevistaInicial");
        }

        public async Task<IActionResult> DetalleEntrevista()
        {
            if (_simulatedRoleId != 1) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .Include(e => e.Student)
                .ThenInclude(s => s.Person)
                .FirstOrDefaultAsync(e => e.StudentId == _simulatedUserId);

            return View("~/Areas/Tutorship/Views/DetalleEntrevista.cshtml", entrevista);
        }

        // ==========================================
        // MÓDULOS DEL MAESTRO (Rol 2)
        // ==========================================
        public async Task<IActionResult> VerEntrevistaAlumno(int id)
        {
            if (_simulatedRoleId != 2) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .Include(e => e.Student)
                .ThenInclude(s => s.Person)
                .FirstOrDefaultAsync(e => e.StudentId == id);

            if (entrevista == null)
            {
                TempData["Mensaje"] = "Este alumno aún no ha llenado su entrevista inicial.";
                return RedirectToAction(nameof(ListaDeAlumnos));
            }

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