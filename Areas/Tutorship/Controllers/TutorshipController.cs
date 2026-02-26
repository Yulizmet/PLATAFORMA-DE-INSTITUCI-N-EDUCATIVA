using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using System.Collections.Generic;

namespace SchoolManager.Areas.Tutorship.Controllers
{
    [Area("Gestion")]
    public class TutorshipController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly int _simulatedRoleId = 2;
        private readonly int _simulatedUserId = 10;

        public TutorshipController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // SEGURIDAD
        public IActionResult AccesoDenegado()
        {
            return Content("No tienes permiso para ver esta pantalla. Tu rol actual es: " + _simulatedRoleId);
        }

        // VISTA PRINCIPAL (Auto-aprovisionamiento integrado)
        public async Task<IActionResult> Controlador()
        {
            ViewBag.RoleId = _simulatedRoleId;

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

            var usuario = await _context.Users.Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == _simulatedUserId);

            ViewBag.NombreUsuario = (usuario != null && usuario.Person != null) ? usuario.Person.FirstName : "Alumno";

            return View("~/Areas/Tutorship/Views/Controlador.cshtml");
        }

        // MÓDULOS DEL ALUMNO (Rol 1)
        public async Task<IActionResult> EntrevistaInicial()
        {
            if (_simulatedRoleId != 1) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;

            var usuario = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == _simulatedUserId);

            if (usuario == null) return NotFound("Usuario no encontrado.");

            var matriculaAlumno = await _context.PreenrollmentGenerals
                .Where(p => p.UserId == _simulatedUserId)
                .Select(p => p.Matricula)
                .FirstOrDefaultAsync();
            ViewBag.Matricula = matriculaAlumno ?? "Sin asignar";

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .FirstOrDefaultAsync(e => e.StudentId == _simulatedUserId);

            ViewBag.Entrevista = entrevista;

            return View("~/Areas/Tutorship/Views/EntrevistaInicial.cshtml", usuario);
        }

        [HttpPost]
        // INTEGRACIÓN AQUÍ: Añadimos IFormFile FotoPerfil como parámetro
        public async Task<IActionResult> GuardarEntrevista(EntrevistaViewModel modelo, IFormFile FotoPerfil)
        {
            if (_simulatedRoleId != 1) return RedirectToAction(nameof(AccesoDenegado));

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .FirstOrDefaultAsync(e => e.StudentId == modelo.UserId);

            if (entrevista == null) return NotFound();

            entrevista.Status = "Completada";
            entrevista.DateCompleted = DateTime.Now;

            
            if (FotoPerfil != null && FotoPerfil.Length > 0)
            {
                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "interview");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (!string.IsNullOrEmpty(entrevista.FilePath) && entrevista.FilePath != "Sin archivo")
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, entrevista.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(FotoPerfil.FileName);
                string fullPath = Path.Combine(folderPath, uniqueFileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await FotoPerfil.CopyToAsync(fileStream);
                }

                entrevista.FilePath = "/uploads/interview/" + uniqueFileName;
            }

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

        public async Task<IActionResult> ListaDeAlumnos(int? grado, string? grupo)
        {
            if (_simulatedRoleId != 2) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;

            ViewBag.GradoSeleccionado = grado;
            ViewBag.GrupoSeleccionado = grupo;

            ViewBag.GradosDisponibles = await _context.grades_GradeGroups
                .Select(g => g.GradeLevelId)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            ViewBag.GruposDisponibles = await _context.grades_GradeGroups
                .Select(g => g.Name)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();



            var query = _context.Users
                .Include(u => u.Person)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 1))
                .AsQueryable();



            if (grado.HasValue && !string.IsNullOrEmpty(grupo))
            {
                var grupoDb = await _context.grades_GradeGroups
                    .FirstOrDefaultAsync(g => g.GradeLevelId == grado.Value && g.Name == grupo);

                if (grupoDb != null)
                {
                    query = query.Where(u => _context.grades_Enrollments
                        .Any(e => e.StudentId == u.UserId && e.GroupId == grupoDb.GroupId));
                }
                else
                {
                    query = query.Where(u => false);
                    ViewBag.MensajeFiltro = "No se encontró ningún grupo " + grado + grupo + " en el sistema.";
                }
            }


            var listaAlumnos = await query.ToListAsync();

            var userIds = listaAlumnos.Select(u => u.UserId).ToList();

            ViewBag.FotosPerfil = await _context.TutorshipInterviews
    .Where(e => userIds.Contains(e.StudentId) && e.FilePath != null && e.FilePath != "Sin archivo")
    .ToDictionaryAsync(e => e.StudentId, e => e.FilePath);


            ViewBag.Matriculas = await _context.PreenrollmentGenerals
                .Where(p => p.UserId != null && userIds.Contains(p.UserId.Value))
                .Select(p => new { p.UserId, p.Matricula })
                .ToDictionaryAsync(p => p.UserId.Value, p => p.Matricula);

            ViewBag.Grupos = await _context.grades_Enrollments
                .Include(e => e.Group)
                .Where(e => userIds.Contains(e.StudentId))
                .ToDictionaryAsync(e => e.StudentId, e => e.Group.GradeLevelId + e.Group.Name);

            return View("~/Areas/Tutorship/Views/ListaDeAlumnos.cshtml", listaAlumnos);
        }

        public IActionResult Asistencia()
        {
            if (_simulatedRoleId != 2) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;
            return View("~/Areas/Tutorship/Views/Asistencia.cshtml");
        }

        public async Task<IActionResult> Seguimiento(string matriculaBuscar)
        {
            if (_simulatedRoleId != 2) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = _simulatedRoleId;

            if (string.IsNullOrEmpty(matriculaBuscar))
            {
                return View("~/Areas/Tutorship/Views/Seguimiento.cshtml");
            }

            var preinscripcion = await _context.PreenrollmentGenerals
                .Where(p => p.Matricula == matriculaBuscar)
                .Select(p => new
                {
                    UserId = p.UserId,
                    Matricula = p.Matricula
                })
                .FirstOrDefaultAsync();

            if (preinscripcion == null || preinscripcion.UserId == null)
            {
                TempData["Error"] = "No se encontró ningún alumno con la matrícula: " + matriculaBuscar;
                return View("~/Areas/Tutorship/Views/Seguimiento.cshtml");
            }

            var alumno = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == preinscripcion.UserId);

            if (alumno == null)
            {
                TempData["Error"] = "El alumno existe pero no tiene datos personales registrados.";
                return View("~/Areas/Tutorship/Views/Seguimiento.cshtml");
            }

            var historial = await _context.TutorshipMonitorings
                .Where(m => m.StudentId == alumno.UserId)
                .OrderByDescending(m => m.Date)
                .ToListAsync();

            ViewBag.Matricula = preinscripcion.Matricula;
            ViewBag.Historial = historial;

            return View("~/Areas/Tutorship/Views/Seguimiento.cshtml", alumno);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarSeguimiento(int studentId, string matricula, string tipo, string observaciones, IFormFile ArchivoAdjunto)
        {
            string rutaArchivoBaseDeDatos = "Sin archivo";

            if (ArchivoAdjunto != null && ArchivoAdjunto.Length > 0)
            {
                string carpetaUploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "seguimiento");

                if (!Directory.Exists(carpetaUploads))
                {
                    Directory.CreateDirectory(carpetaUploads);
                }

                string nombreArchivoUnico = Guid.NewGuid().ToString() + "_" + ArchivoAdjunto.FileName;

                string rutaFisicaCompleta = Path.Combine(carpetaUploads, nombreArchivoUnico);

                using (var stream = new FileStream(rutaFisicaCompleta, FileMode.Create))
                {
                    await ArchivoAdjunto.CopyToAsync(stream);
                }

                rutaArchivoBaseDeDatos = "/uploads/seguimiento/" + nombreArchivoUnico;
            }

            var nuevoReporte = new tutorship_monitoring
            {
                StudentId = studentId,
                TeacherId = _simulatedUserId,
                Date = DateTime.Now,
                PerformanceLevel = tipo ?? "General",
                DetailedObservations = observaciones ?? "Sin observaciones",
                ActionPlan = "N/A",
                FilePath = rutaArchivoBaseDeDatos
            };

            _context.TutorshipMonitorings.Add(nuevoReporte);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Reporte guardado correctamente.";

            return RedirectToAction("Seguimiento", new { matriculaBuscar = matricula });
        }
    }
}