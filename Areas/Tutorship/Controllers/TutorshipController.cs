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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SchoolManager.Areas.Tutorship.Controllers
{
    [Area("Gestion")]
    [Authorize]
    public class TutorshipController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private int LoggedUserId => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        private int LoggedRoleId
        {
            get
            {
                if (User.IsInRole("Student")) return 1;
                if (User.IsInRole("Teacher")) return 2;
                if (User.IsInRole("Administrator")) return 3;
                return 0; 
            }
        }

        public TutorshipController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult AccesoDenegado()
        {
            return Content("No tienes permiso para ver esta pantalla. Tu rol actual es: " + LoggedRoleId);
        }

        public async Task<IActionResult> Controlador()
        {
            ViewBag.RoleId = LoggedRoleId;

            var entrevistaExistente = await _context.TutorshipInterviews.FirstOrDefaultAsync(e => e.StudentId == LoggedUserId);

            if (entrevistaExistente == null && LoggedRoleId == 1) 
            {
                var nuevaEntrevista = new tutorship_interview
                {
                    StudentId = LoggedUserId, 
                    Status = "Pendiente",
                    FilePath = "Sin archivo",
                    DateCompleted = DateTime.Now
                };
                _context.TutorshipInterviews.Add(nuevaEntrevista);
                await _context.SaveChangesAsync();
            }

            var usuario = await _context.Users.Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == LoggedUserId); 

            ViewBag.NombreUsuario = (usuario != null && usuario.Person != null) ? usuario.Person.FirstName : "Usuario";

            return View("~/Areas/Tutorship/Views/Controlador.cshtml");
        }

        public async Task<IActionResult> EntrevistaInicial()
        {
            if (LoggedRoleId != 1) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = LoggedRoleId;

            var usuario = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == LoggedUserId); 

            if (usuario == null) return NotFound("Usuario no encontrado.");

            var matriculaAlumno = await _context.PreenrollmentGenerals
                .Where(p => p.UserId == LoggedUserId)
                .Select(p => p.Matricula)
                .FirstOrDefaultAsync();
            ViewBag.Matricula = matriculaAlumno ?? "Sin asignar";

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .FirstOrDefaultAsync(e => e.StudentId == LoggedUserId);

            ViewBag.Entrevista = entrevista;

            return View("~/Areas/Tutorship/Views/EntrevistaInicial.cshtml", usuario);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarEntrevista(EntrevistaViewModel modelo, IFormFile FotoPerfil)
        {
            if (LoggedRoleId != 1) return RedirectToAction(nameof(AccesoDenegado));

            if (modelo.UserId != LoggedUserId) return RedirectToAction(nameof(AccesoDenegado));

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .FirstOrDefaultAsync(e => e.StudentId == LoggedUserId); 

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
            if (LoggedRoleId != 1) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = LoggedRoleId;

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .Include(e => e.Student)
                .ThenInclude(s => s.Person)
                .FirstOrDefaultAsync(e => e.StudentId == LoggedUserId); 

            return View("~/Areas/Tutorship/Views/DetalleEntrevista.cshtml", entrevista);
        }

        public async Task<IActionResult> VerEntrevistaAlumno(int id)
        {
            if (LoggedRoleId != 2) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = LoggedRoleId;


            bool esTutor = await _context.Tutorships.AnyAsync(t => t.StudentId == id && t.TeacherId == LoggedUserId);
            if (!esTutor)
            {
                TempData["Mensaje"] = "Acceso denegado: Este alumno no pertenece a tu grupo de tutoría.";
                return RedirectToAction(nameof(ListaDeAlumnos));
            }

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
            if (LoggedRoleId != 2 && LoggedRoleId != 3) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = LoggedRoleId;

            ViewBag.GradoSeleccionado = grado;
            ViewBag.GrupoSeleccionado = grupo;

            IQueryable<grades_group> gruposQuery = _context.grades_GradeGroups;
            IQueryable<users_user> alumnosQuery = _context.Users
                .Include(u => u.Person)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 1));

            if (LoggedRoleId == 2)
            {
                gruposQuery = _context.grades_Enrollments
                    .Include(e => e.Group)
                    .Where(e => _context.Tutorships.Any(t => t.StudentId == e.StudentId && t.TeacherId == LoggedUserId))
                    .Select(e => e.Group)
                    .Distinct();

                alumnosQuery = alumnosQuery.Where(u => _context.Tutorships.Any(t => t.StudentId == u.UserId && t.TeacherId == LoggedUserId));
            }
            else if (LoggedRoleId == 3)
            {
            }

            var gruposDisponibles = await gruposQuery.ToListAsync();
            ViewBag.GradosDisponibles = gruposDisponibles.Select(g => g.GradeLevelId).Distinct().OrderBy(g => g).ToList();
            ViewBag.GruposDisponibles = gruposDisponibles.Select(g => g.Name).Distinct().OrderBy(g => g).ToList();

            if (grado.HasValue && !string.IsNullOrEmpty(grupo))
            {
                var grupoDb = await _context.grades_GradeGroups
                    .FirstOrDefaultAsync(g => g.GradeLevelId == grado.Value && g.Name == grupo);

                if (grupoDb != null)
                {
                    alumnosQuery = alumnosQuery.Where(u => _context.grades_Enrollments
                        .Any(e => e.StudentId == u.UserId && e.GroupId == grupoDb.GroupId));
                }
                else
                {
                    alumnosQuery = alumnosQuery.Where(u => false);
                    ViewBag.MensajeFiltro = "No se encontró ningún grupo " + grado + grupo + " en el sistema.";
                }
            }

            var listaAlumnos = await alumnosQuery.ToListAsync();
            var userIds = listaAlumnos.Select(u => u.UserId).ToList();

            ViewBag.FotosPerfil = await _context.TutorshipInterviews
                .Where(e => userIds.Contains(e.StudentId) && e.FilePath != null && e.FilePath != "Sin archivo")
                .ToDictionaryAsync(e => e.StudentId, e => e.FilePath);

            ViewBag.Matriculas = await _context.PreenrollmentGenerals
                .Where(p => p.UserId != null && userIds.Contains(p.UserId.Value))
                .Select(p => new { p.UserId, p.Matricula })
                .ToDictionaryAsync(p => p.UserId.Value, p => p.Matricula);

            var enrollments = await _context.grades_Enrollments
                .Include(e => e.Group)
                .Where(e => userIds.Contains(e.StudentId))
                .ToListAsync();

            ViewBag.Grupos = enrollments
                .GroupBy(e => e.StudentId)
                .ToDictionary(g => g.Key, g => g.First().Group.GradeLevelId + g.First().Group.Name);

            return View("~/Areas/Tutorship/Views/ListaDeAlumnos.cshtml", listaAlumnos);
        }


        [HttpGet]
        public async Task<IActionResult> Asistencia(DateTime? fecha, int? groupId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (LoggedRoleId != 2 && LoggedRoleId != 3) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = LoggedRoleId;

            DateTime fechaSeleccionada = fecha ?? DateTime.Now.Date;
            DateTime inicioRango = fechaInicio ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime finRango = fechaFin ?? DateTime.Now.Date;

            ViewBag.FechaSeleccionada = fechaSeleccionada.ToString("yyyy-MM-dd");
            ViewBag.FechaInicio = inicioRango.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = finRango.ToString("yyyy-MM-dd");

            List<grades_group> gruposDisponibles = new List<grades_group>();

            if (LoggedRoleId == 2)
            {
                gruposDisponibles = await _context.grades_Enrollments
                    .Include(e => e.Group)
                    .Where(e => _context.Tutorships.Any(t => t.StudentId == e.StudentId && t.TeacherId == LoggedUserId))
                    .Select(e => e.Group)
                    .Distinct()
                    .ToListAsync();

                if (!groupId.HasValue && gruposDisponibles.Any())
                {
                    groupId = gruposDisponibles.First().GroupId;
                }
            }
            else if (LoggedRoleId == 3)
            {
                gruposDisponibles = await _context.grades_GradeGroups
                    .OrderBy(g => g.GradeLevelId).ThenBy(g => g.Name)
                    .ToListAsync();
            }

            ViewBag.GruposDisponibles = gruposDisponibles;
            ViewBag.GrupoSeleccionado = groupId;

            List<users_user> alumnos = new List<users_user>();

            if (groupId.HasValue)
            {
                var query = _context.Users
                    .Where(u => u.UserRoles.Any(ur => ur.RoleId == 1) &&
                                _context.grades_Enrollments.Any(e => e.StudentId == u.UserId && e.GroupId == groupId.Value));

                if (LoggedRoleId == 2)
                {
                    query = query.Where(u => _context.Tutorships.Any(t => t.StudentId == u.UserId && t.TeacherId == LoggedUserId));
                }

                alumnos = await query.Select(u => new users_user
                {
                    UserId = u.UserId,
                    Person = new users_person
                    {
                        FirstName = u.Person.FirstName,
                        LastNamePaternal = u.Person.LastNamePaternal,
                        LastNameMaternal = u.Person.LastNameMaternal
                    }
                }).ToListAsync();

                var userIds = alumnos.Select(u => u.UserId).ToList();


                ViewBag.Matriculas = await _context.PreenrollmentGenerals
                    .Where(p => p.UserId != null && userIds.Contains(p.UserId.Value))
                    .Select(p => new { p.UserId, p.Matricula })
                    .ToDictionaryAsync(p => p.UserId.Value, p => p.Matricula);

                ViewBag.FaltasTotales = await _context.TutorshipAttendances
                    .Where(a => a.GroupId == groupId.Value
                             && !a.IsPresent
                             && userIds.Contains(a.StudentId)
                             && a.Date.Date >= inicioRango.Date
                             && a.Date.Date <= finRango.Date)
                    .GroupBy(a => a.StudentId)
                    .Select(g => new { StudentId = g.Key, Faltas = g.Count() })
                    .ToDictionaryAsync(x => x.StudentId, x => x.Faltas);

                ViewBag.AsistenciaHoy = await _context.TutorshipAttendances
                    .Where(a => a.GroupId == groupId.Value && a.Date.Date == fechaSeleccionada.Date)
                    .ToDictionaryAsync(a => a.StudentId, a => a.IsPresent);
            }

            return View("~/Areas/Tutorship/Views/Asistencia.cshtml", alumnos);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarAsistencia(List<int> studentIds, List<bool> isPresent, int groupId, DateTime fecha, DateTime fechaInicio, DateTime fechaFin)
        {

            if (LoggedRoleId != 2 && LoggedRoleId != 3) return RedirectToAction(nameof(AccesoDenegado));

            if (studentIds == null || !studentIds.Any())
            {
                TempData["Error"] = "No hay alumnos para guardar asistencia.";
                return RedirectToAction(nameof(Asistencia), new
                {
                    fecha = fecha.ToString("yyyy-MM-dd"),
                    groupId = groupId,
                    fechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                    fechaFin = fechaFin.ToString("yyyy-MM-dd")
                });
            }

            bool asistenciaYaTomada = await _context.TutorshipAttendances
                .AnyAsync(a => a.GroupId == groupId && a.Date.Date == fecha.Date);

            if (asistenciaYaTomada)
            {
                TempData["Error"] = "La asistencia para este grupo ya fue registrada en la fecha seleccionada.";
                return RedirectToAction(nameof(Asistencia), new
                {
                    fecha = fecha.ToString("yyyy-MM-dd"),
                    groupId = groupId,
                    fechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                    fechaFin = fechaFin.ToString("yyyy-MM-dd")
                });
            }

            var registrosAsistencia = new List<tutorship_attendance>();

            for (int i = 0; i < studentIds.Count; i++)
            {
                registrosAsistencia.Add(new tutorship_attendance
                {
                    StudentId = studentIds[i],
                    TeacherId = LoggedUserId,
                    Date = fecha,
                    IsPresent = isPresent[i],
                    GroupId = groupId
                });
            }

            _context.TutorshipAttendances.AddRange(registrosAsistencia);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Asistencia guardada correctamente.";


            return RedirectToAction(nameof(Asistencia), new
            {
                fecha = fecha.ToString("yyyy-MM-dd"),
                groupId = groupId,
                fechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                fechaFin = fechaFin.ToString("yyyy-MM-dd")
            });
        }

        public async Task<IActionResult> Seguimiento(string matriculaBuscar)
        {
            if (LoggedRoleId != 2) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = LoggedRoleId;

            if (string.IsNullOrEmpty(matriculaBuscar))
            {
                return View("~/Areas/Tutorship/Views/Seguimiento.cshtml");
            }

            var preinscripcion = await _context.PreenrollmentGenerals
                .Where(p => p.Matricula == matriculaBuscar)
                .Select(p => new { UserId = p.UserId, Matricula = p.Matricula })
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


            bool esTutor = await _context.Tutorships.AnyAsync(t => t.StudentId == alumno.UserId && t.TeacherId == LoggedUserId);
            if (!esTutor)
            {
                TempData["Error"] = "Acceso denegado: Puedes ver que el alumno existe, pero no pertenece a tu grupo de tutoría para dejar reportes.";
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
                TeacherId = LoggedUserId, 
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

        public async Task<IActionResult> AsignarTutores()
        {
            if (LoggedRoleId != 3) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleId = LoggedRoleId;

            ViewBag.Maestros = await _context.Users
                .Include(u => u.Person)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 2))
                .ToListAsync();

            ViewBag.Grupos = await _context.grades_GradeGroups
                .OrderBy(g => g.GradeLevelId).ThenBy(g => g.Name)
                .ToListAsync();

            return View("~/Areas/Tutorship/Views/AsignarTutores.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> ReiniciarEntrevistasCuatrimestre()
        {
            if (LoggedRoleId != 3) return RedirectToAction(nameof(AccesoDenegado));

            var entrevistas = await _context.TutorshipInterviews
                .Where(e => e.Status == "Completada")
                .ToListAsync();

            if (entrevistas.Any())
            {
                foreach (var entrevista in entrevistas)
                {
                    entrevista.Status = "Requiere Actualizacion";
                }

                _context.TutorshipInterviews.UpdateRange(entrevistas);
                await _context.SaveChangesAsync();
            }

            TempData["Exito"] = $"Se ha solicitado a {entrevistas.Count} alumnos que actualicen su entrevista para el nuevo ciclo.";

            return RedirectToAction("Controlador");
        }

        [HttpGet]
        public async Task<IActionResult> ReporteEntrevistas(string? filtroEstatus)
        {
            if (LoggedRoleId != 2 && LoggedRoleId != 3) return RedirectToAction(nameof(AccesoDenegado));

            ViewBag.FiltroActual = filtroEstatus;
            ViewBag.RolActual = LoggedRoleId;

            var queryAlumnos = _context.Users
                .Include(u => u.Person)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 1));

            if (LoggedRoleId == 2)
            {
                queryAlumnos = queryAlumnos.Where(u => _context.Tutorships.Any(t => t.StudentId == u.UserId && t.TeacherId == LoggedUserId));
            }

            var todosLosAlumnos = await queryAlumnos.ToListAsync();
            var studentIds = todosLosAlumnos.Select(u => u.UserId).ToList();

            // 4. Estatus de entrevistas
            var dictEstatus = await _context.TutorshipInterviews
                .Where(e => studentIds.Contains(e.StudentId))
                .ToDictionaryAsync(e => e.StudentId, e => e.Status);

            foreach (var id in studentIds)
            {
                if (!dictEstatus.ContainsKey(id))
                {
                    dictEstatus[id] = "Pendiente";
                }
            }


            if (!string.IsNullOrEmpty(filtroEstatus))
            {
                var idsFiltrados = dictEstatus.Where(kvp => kvp.Value == filtroEstatus).Select(kvp => kvp.Key).ToList();
                todosLosAlumnos = todosLosAlumnos.Where(a => idsFiltrados.Contains(a.UserId)).ToList();
            }

            var filteredStudentIds = todosLosAlumnos.Select(u => u.UserId).ToList();

            ViewBag.Matriculas = await _context.PreenrollmentGenerals
                .Where(p => p.UserId != null && filteredStudentIds.Contains(p.UserId.Value))
                .Select(p => new { UserId = p.UserId.Value, Matricula = p.Matricula })
                .ToDictionaryAsync(p => p.UserId, p => p.Matricula);

            ViewBag.Grupos = await _context.grades_Enrollments
                .Include(e => e.Group)
                .Where(e => filteredStudentIds.Contains(e.StudentId))
                .ToDictionaryAsync(e => e.StudentId, e => e.Group.GradeLevelId + e.Group.Name);

            ViewBag.EstatusEntrevistas = dictEstatus;

            return View("~/Areas/Tutorship/Views/ReporteEntrevistas.cshtml", todosLosAlumnos);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarAsignacionTutor(int teacherId, int groupId)
        {
            if (LoggedRoleId != 3) return RedirectToAction(nameof(AccesoDenegado));

            var alumnosGrupo = await _context.grades_Enrollments
                .Where(e => e.GroupId == groupId)
                .Select(e => e.StudentId)
                .ToListAsync();

            if (!alumnosGrupo.Any())
            {
                TempData["Error"] = "El grupo seleccionado no tiene alumnos inscritos actualmente.";
                return RedirectToAction(nameof(AsignarTutores));
            }

            foreach (var studentId in alumnosGrupo)
            {
                var tutoriaExistente = await _context.Tutorships.FirstOrDefaultAsync(t => t.StudentId == studentId);

                if (tutoriaExistente != null)
                {
                    tutoriaExistente.TeacherId = teacherId;

                    if (string.IsNullOrEmpty(tutoriaExistente.Topic))
                    {
                        tutoriaExistente.Topic = "Tutoría Asignada";
                    }

                    _context.Tutorships.Update(tutoriaExistente);
                }
                else
                {
                    _context.Tutorships.Add(new tutorship
                    {
                        StudentId = studentId,
                        TeacherId = teacherId,
                        Topic = "Tutoría Asignada",
                        Date = DateTime.Now,
                    });
                }

            }



            await _context.SaveChangesAsync();

            var grupoDb = await _context.grades_GradeGroups.FindAsync(groupId);
            string nombreGrupo = grupoDb != null ? $"{grupoDb.GradeLevelId}{grupoDb.Name}" : "seleccionado";

            TempData["Exito"] = $"Tutor asignado correctamente a los {alumnosGrupo.Count} alumnos del grupo {nombreGrupo}.";

            return RedirectToAction(nameof(AsignarTutores));
        }
    }
}