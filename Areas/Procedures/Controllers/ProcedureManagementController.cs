using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class ProcedureManagementController : Controller
    {
        private readonly AppDbContext _context;

        public ProcedureManagementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureStatus)
                .Include(r => r.ProcedureDocuments)
                //.Include(r => r.User)
                    //.ThenInclude(u => u.Person)
                .OrderByDescending(r => r.DateUpdated)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> Monitoring(int id)
        {
            var request = await _context.ProcedureRequest
        .Include(r => r.ProcedureType)
        .Include(r => r.ProcedureStatus)
        .Include(r => r.ProcedureDocuments)
        .Include(r => r.ProcedureMonitorings)
            .ThenInclude(m => m.ProcedureStatus)
        .Include(r => r.ProcedureMonitorings)
            .ThenInclude(m => m.User)
        .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            ViewBag.Statuses = new SelectList(await _context.ProcedureStatus.ToListAsync(), "Id", "Name", request.IdStatus);

            return PartialView("_Monitoring", request);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int requestId, int newStatusId, string adminComment)
        {
            try
            {
                var request = await _context.ProcedureRequest.FindAsync(requestId);
                if (request == null) return Json(new { success = false, message = "Solicitud no encontrada" });

                request.IdStatus = newStatusId;
                request.DateUpdated = DateTime.Now;

                var monitoring = new procedure_monitoring
                {
                    IdProcedure = requestId,
                    IdStatus = newStatusId,
                    Comment = adminComment,
                    DateUpdated = DateTime.Now,
                    IdUser = 1
                };

                _context.ProcedureMonitoring.Add(monitoring);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Estatus actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}