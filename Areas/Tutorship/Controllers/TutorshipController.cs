using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

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

            if (usuario == null) return NotFound("Usuario no encontrado.");

            var matriculaAlumno = await _context.PreenrollmentGenerals
                .Where(p => p.UserId == usuarioIdDePrueba)
                .Select(p => p.Matricula)
                .FirstOrDefaultAsync();
            ViewBag.Matricula = matriculaAlumno ?? "Sin asignar";

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers) 
                .FirstOrDefaultAsync(e => e.StudentId == usuarioIdDePrueba);

            ViewBag.Entrevista = entrevista;

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

        public async Task<IActionResult> Controlador()
        {
            int usuarioAutenticadoId = 15;

            
            var entrevistaExistente = await _context.TutorshipInterviews.FirstOrDefaultAsync(e => e.StudentId == usuarioAutenticadoId);

            if (entrevistaExistente == null)
            {
                var nuevaEntrevista = new tutorship_interview
                {
                    StudentId = usuarioAutenticadoId,
                    Status = "Pendiente",
                    FilePath = "Sin archivo",
                    DateCompleted = DateTime.Now 
                };

                _context.TutorshipInterviews.Add(nuevaEntrevista);
                await _context.SaveChangesAsync(); 
            }

            var usuario = await _context.Users.Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == usuarioAutenticadoId);

            if (usuario != null && usuario.Person != null)
            {
                ViewBag.NombreUsuario = usuario.Person.FirstName;
            }
            else
            {
                ViewBag.NombreUsuario = "Alumno";
            }

            return View("~/Areas/Tutorship/Views/Controlador.cshtml");
        }
        public async Task<IActionResult> ListaDeAlumnos()
        {
            var listaAlumnos = await _context.Users
        .Include(u => u.Person)
        .ToListAsync();

            return View("~/Areas/Tutorship/Views/ListaDeAlumnos.cshtml", listaAlumnos);
        }
        [HttpPost]
        public async Task<IActionResult> GuardarEntrevista(EntrevistaViewModel modelo)
        {
            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .FirstOrDefaultAsync(e => e.StudentId == modelo.UserId);

            if (entrevista == null) return NotFound(); 
            entrevista.Status = "Completada";
            entrevista.DateCompleted = DateTime.Now;
            _context.TutorshipInterviews.Update(entrevista);

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
            else
            {
                
                string catPersonal = "Aspectos Personales y Familiares";
                string catAcademico = "Antecedentes y Hábitos Académicos";

                var listaRespuestas = new List<tutorship_interview_answer>
        {
            new tutorship_interview_answer { InterviewId = entrevista.InterviewId, QuestionCategory = catPersonal, QuestionText = "1. Estado Civil", AnswerText = modelo.EstadoCivil ?? "N/A" },
        };
                _context.TutorshipInterviewAnswers.AddRange(listaRespuestas);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("EntrevistaInicial");
        }
    }
}
