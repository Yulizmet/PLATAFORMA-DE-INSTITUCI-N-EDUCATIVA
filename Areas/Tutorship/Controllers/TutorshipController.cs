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


        
    }
}