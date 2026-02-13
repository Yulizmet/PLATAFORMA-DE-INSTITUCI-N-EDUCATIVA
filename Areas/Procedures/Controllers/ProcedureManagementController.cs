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
            var history = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureStatus)
                .Include(r => r.ProcedureDocuments)
                .OrderByDescending(r => r.DateUpdated)
                .ToListAsync();

            return View(history);
        }

        public async Task<IActionResult> Seguimiento(int id)
        {
            var request = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureStatus)
                .Include(r => r.ProcedureDocuments)
                .Include(r => r.ProcedureMonitorings!)
                    .ThenInclude(m => m.ProcedureStatus)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null) return NotFound();

            ViewBag.Statuses = new SelectList(await _context.ProcedureStatus.ToListAsync(), "Id", "Name", request.IdStatus);

            return PartialView("_Seguimiento", request);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStep(int requestId, int newStatusId, string adminComment)
        {
            var request = await _context.ProcedureRequest.FindAsync(requestId);
            if (request == null) return Json(new { success = false, message = "Solicitud no encontrada" });

            request.IdStatus = newStatusId;
            request.DateUpdated = DateTime.Now;

            var step = new procedure_monitoring
            {
                IdProcedure = requestId,
                //IdUser = User.Identity?.Name ?? "Admin_System", 
                IdStatus = newStatusId,
                Comment = adminComment,
                DateUpdated = DateTime.Now
            };

            _context.ProcedureMonitoring.Add(step);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}