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
    public class EntrevistaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private int LoggedUserId => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        private string LoggedRoleName => User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Ninguno";

        public EntrevistaController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult AccesoDenegado()
        {
            return Content("No tienes permiso para ver esta pantalla. Tu rol actual es: " + LoggedRoleName);
        }

        private async Task<int> GetDbRoleIdByNameAsync(string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            return role?.RoleId ?? 0;
        }

        public async Task<IActionResult> EntrevistaInicial()
        {
            if (!User.IsInRole("Student")) return RedirectToAction(nameof(AccesoDenegado));

            ViewBag.RoleName = LoggedRoleName;

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
            if (!User.IsInRole("Student")) return RedirectToAction(nameof(AccesoDenegado));

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
            if (!User.IsInRole("Student")) return RedirectToAction(nameof(AccesoDenegado));
            ViewBag.RoleName = LoggedRoleName;

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .Include(e => e.Student)
                .ThenInclude(s => s.Person)
                .FirstOrDefaultAsync(e => e.StudentId == LoggedUserId);

            return View("~/Areas/Tutorship/Views/DetalleEntrevista.cshtml", entrevista);
        }

        public async Task<IActionResult> VerEntrevistaAlumno(int id)
        {
            if (!User.IsInRole("Teacher") && !User.IsInRole("Administrator") && !User.IsInRole("Master"))
                return RedirectToAction("AccesoDenegado");

            ViewBag.RoleName = LoggedRoleName;

            if (User.IsInRole("Teacher"))
            {
                bool esTutor = await _context.Tutorships.AnyAsync(t => t.StudentId == id && t.TeacherId == LoggedUserId);
                if (!esTutor)
                {
                    TempData["Mensaje"] = "Acceso denegado: Este alumno no pertenece a tu grupo de tutoría.";
                    return RedirectToAction("ListaDeAlumnos", "Alumnos");
                }
            }

            var entrevista = await _context.TutorshipInterviews
                .Include(e => e.Answers)
                .FirstOrDefaultAsync(e => e.StudentId == id);

            if (entrevista == null)
            {
                TempData["Mensaje"] = "Este alumno aún no ha llenado su entrevista inicial.";
                return RedirectToAction("ListaDeAlumnos", "Alumnos");
            }

            var alumno = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == id);

            entrevista.Student = alumno;

            ViewBag.FotoPerfil = (entrevista.FilePath != null && entrevista.FilePath != "Sin archivo")
                                 ? entrevista.FilePath : "";

            var matriculaAlumno = await _context.PreenrollmentGenerals
                .Where(p => p.UserId == id)
                .Select(p => p.Matricula)
                .FirstOrDefaultAsync();

            ViewBag.Matricula = matriculaAlumno ?? "";

            var inscripcion = await _context.grades_Enrollments
                .Include(e => e.Group)
                .FirstOrDefaultAsync(e => e.StudentId == id);

            ViewBag.Grupo = inscripcion?.Group != null
                            ? $"Grado {inscripcion.Group.GradeLevelId} Grupo {inscripcion.Group.Name}" : "";

            return View("~/Areas/Tutorship/Views/DetalleEntrevista.cshtml", entrevista);
        }

        [HttpGet]
        public async Task<IActionResult> ReporteEntrevistas(string? filtroEstatus)
        {
            if (!User.IsInRole("Teacher") && !User.IsInRole("Administrator") && !User.IsInRole("Master"))
                return RedirectToAction(nameof(AccesoDenegado));

            ViewBag.FiltroActual = filtroEstatus;
            ViewBag.RolActual = LoggedRoleName; 

            int dbStudentRoleId = await GetDbRoleIdByNameAsync("Student");

            var queryAlumnos = _context.Users
                .Include(u => u.Person)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == dbStudentRoleId));

            if (User.IsInRole("Teacher"))
            {
                queryAlumnos = queryAlumnos.Where(u => _context.Tutorships.Any(t => t.StudentId == u.UserId && t.TeacherId == LoggedUserId));
            }

            var todosLosAlumnos = await queryAlumnos.ToListAsync();
            var studentIds = todosLosAlumnos.Select(u => u.UserId).ToList();

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

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> ObtenerDataReportes()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault() ?? "0";
            var length = Request.Form["length"].FirstOrDefault() ?? "25";
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault() ?? "asc";
            var filtroEstatus = Request.Form["filtroEstatus"].FirstOrDefault();

            int pageSize = int.Parse(length);
            int skip = int.Parse(start);

            int dbStudentRoleId = await GetDbRoleIdByNameAsync("Student");

            var query = from u in _context.Users
                        where u.UserRoles.Any(ur => ur.RoleId == dbStudentRoleId)
                        join p in _context.PreenrollmentGenerals on u.UserId equals p.UserId into pg
                        from p in pg.DefaultIfEmpty()
                        join e in _context.grades_Enrollments.Include(ge => ge.Group) on u.UserId equals e.StudentId into eg
                        from e in eg.DefaultIfEmpty()
                        join t in _context.TutorshipInterviews on u.UserId equals t.StudentId into tg
                        from t in tg.DefaultIfEmpty()
                        select new
                        {
                            UserId = u.UserId,
                            Nombre = u.Person.FirstName + " " + u.Person.LastNamePaternal + " " + u.Person.LastNameMaternal,
                            Email = u.Email,
                            Matricula = p != null ? p.Matricula : "Sin asignar",
                            Grupo = e != null ? e.Group.GradeLevelId + e.Group.Name : "Sin grupo",
                            Estatus = t != null && t.Status != null ? t.Status : "Pendiente"
                        };

            if (User.IsInRole("Teacher"))
            {
                query = query.Where(x => _context.Tutorships.Any(t => t.StudentId == x.UserId && t.TeacherId == LoggedUserId));
            }

            if (!string.IsNullOrEmpty(filtroEstatus))
            {
                query = query.Where(x => x.Estatus == filtroEstatus);
            }

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => x.Nombre.ToLower().Contains(searchValue) ||
                                         x.Matricula.ToLower().Contains(searchValue));
            }

            int recordsTotal = await _context.Users.CountAsync(u => u.UserRoles.Any(ur => ur.RoleId == dbStudentRoleId));
            int recordsFiltered = await query.CountAsync();

            bool asc = sortDirection == "asc";
            query = sortColumnIndex switch
            {
                "0" => asc ? query.OrderBy(x => x.Nombre) : query.OrderByDescending(x => x.Nombre),
                "1" => asc ? query.OrderBy(x => x.Matricula) : query.OrderByDescending(x => x.Matricula),
                "4" => asc ? query.OrderBy(x => x.Estatus) : query.OrderByDescending(x => x.Estatus),
                _ => query.OrderBy(x => x.Nombre)
            };

            var datosPaginados = await query.Skip(skip).Take(pageSize).ToListAsync();

            return Json(new
            {
                draw = draw,
                recordsFiltered = recordsFiltered,
                recordsTotal = recordsTotal,
                data = datosPaginados
            });
        }
    }
}
