using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace SchoolManager.Areas.Tutorship.Controllers
{
    [Area("Gestion")]
    [Authorize]
    public class AlumnosController : Controller
    {
        private readonly AppDbContext _context;

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

        public AlumnosController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult AccesoDenegado()
        {
            return Content("No tienes permiso para ver esta pantalla. Tu rol actual es: " + LoggedRoleId);
        }

        // =======================================================
        // MÓDULO DE ALUMNOS (Directorio y Asignaciones)
        // =======================================================

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

        [HttpGet]
        public async Task<IActionResult> ObtenerDataAlumnos()
        {
            try
            {
                var draw = Request.Query["draw"].FirstOrDefault();
                var start = Request.Query["start"].FirstOrDefault() ?? "0";
                var length = Request.Query["length"].FirstOrDefault() ?? "10";
                var searchValue = Request.Query["search[value]"].FirstOrDefault()?.ToLower();
                var sortDirection = Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";

                var gradoFilter = Request.Query["grado"].FirstOrDefault();
                var grupoFilter = Request.Query["grupo"].FirstOrDefault();

                int pageSize = int.Parse(length);
                int skip = int.Parse(start);

                var query = from u in _context.Users
                            where u.UserRoles.Any(ur => ur.RoleId == 1)
                            join p in _context.PreenrollmentGenerals on u.UserId equals p.UserId into pg
                            from p in pg.DefaultIfEmpty()
                            join e in _context.grades_Enrollments.Include(ge => ge.Group) on u.UserId equals e.StudentId into eg
                            from e in eg.DefaultIfEmpty()
                            join t in _context.TutorshipInterviews on u.UserId equals t.StudentId into tg
                            from t in tg.DefaultIfEmpty()
                            select new
                            {
                                UserId = u.UserId,
                                Nombre = u.Person.FirstName, 
                                NombreCompleto = u.Person.FirstName + " " + u.Person.LastNamePaternal + " " + u.Person.LastNameMaternal,
                                Matricula = p != null ? p.Matricula : "Sin Matrícula",
                                Grado = e != null ? (int?)e.Group.GradeLevelId : null,
                                GrupoNombre = e != null ? e.Group.Name : null,
                                GrupoTexto = e != null ? e.Group.GradeLevelId + e.Group.Name : "Sin Grupo",
                                Foto = t != null && t.FilePath != null && t.FilePath != "Sin archivo" ? t.FilePath : ""
                            };

                if (LoggedRoleId == 2)
                {
                    query = query.Where(x => _context.Tutorships.Any(t => t.StudentId == x.UserId && t.TeacherId == LoggedUserId));
                }

                // 4. Aplicar Filtros del usuario
                if (!string.IsNullOrEmpty(gradoFilter) && int.TryParse(gradoFilter, out int gradoId))
                {
                    query = query.Where(x => x.Grado == gradoId);
                }

                if (!string.IsNullOrEmpty(grupoFilter))
                {
                    query = query.Where(x => x.GrupoNombre == grupoFilter);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(x => x.NombreCompleto.ToLower().Contains(searchValue) ||
                                             x.Matricula.ToLower().Contains(searchValue));
                }

                int recordsFiltered = await query.CountAsync();

                bool asc = sortDirection == "asc";
                query = asc ? query.OrderBy(x => x.NombreCompleto) : query.OrderByDescending(x => x.NombreCompleto);

                var datosPaginados = await query.Skip(skip).Take(pageSize).ToListAsync();
                int recordsTotal = await _context.Users.CountAsync(u => u.UserRoles.Any(ur => ur.RoleId == 1));

                return Json(new
                {
                    draw = draw,
                    recordsFiltered = recordsFiltered,
                    recordsTotal = recordsTotal,
                    data = datosPaginados
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

    }
}