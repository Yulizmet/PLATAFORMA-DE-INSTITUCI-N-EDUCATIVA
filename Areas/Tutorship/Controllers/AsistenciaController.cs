using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
    public class AsistenciaController : Controller
    {
        private readonly AppDbContext _context;

        // Propiedades auxiliares para usuario y rol
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

        public AsistenciaController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult AccesoDenegado()
        {
            return Content("No tienes permiso para ver esta pantalla. Tu rol actual es: " + LoggedRoleId);
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

                var registrosPeriodo = await _context.TutorshipAttendances
                    .Where(a => a.GroupId == groupId.Value
                             && userIds.Contains(a.StudentId)
                             && a.Date.Date >= inicioRango.Date
                             && a.Date.Date <= finRango.Date)
                    .Select(a => new { a.StudentId, a.IsPresent, a.Date })
                    .ToListAsync();

                ViewBag.DetalleFaltas = registrosPeriodo
                    .Where(a => !a.IsPresent)
                    .GroupBy(a => a.StudentId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(a => a.Date.ToString("dd/MMM")).ToList()
                    );

                ViewBag.DetalleAsistencias = registrosPeriodo
                    .Where(a => a.IsPresent)
                    .GroupBy(a => a.StudentId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(a => a.Date.ToString("dd/MMM")).ToList()
                    );

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
                TempData["AsistenciaError"] = "No hay alumnos para procesar asistencia.";
                return RedirectToAction(nameof(Asistencia), new { fecha = fecha.ToString("yyyy-MM-dd"), groupId, fechaInicio = fechaInicio.ToString("yyyy-MM-dd"), fechaFin = fechaFin.ToString("yyyy-MM-dd") });
            }

            var asistenciasExistentes = await _context.TutorshipAttendances
                .Where(a => a.GroupId == groupId && a.Date.Date == fecha.Date)
                .ToListAsync();

            bool esActualizacion = asistenciasExistentes.Any();

            if (esActualizacion)
            {
                _context.TutorshipAttendances.RemoveRange(asistenciasExistentes);
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

            if (esActualizacion)
            {
                // ¡CORREGIDO AQUÍ!
                TempData["AsistenciaExito"] = "La asistencia fue actualizada correctamente.";
            }
            else
            {
                TempData["AsistenciaExito"] = "Asistencia guardada correctamente.";
            }

            return RedirectToAction(nameof(Asistencia), new
            {
                fecha = fecha.ToString("yyyy-MM-dd"),
                groupId = groupId,
                fechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                fechaFin = fechaFin.ToString("yyyy-MM-dd")
            });
        }
    }
}