using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                Total = await _context.ProcedureRequest.CountAsync(),
                Nuevos = await _context.ProcedureRequest.CountAsync(r => r.IdStatus == 1),
                Finalizados = await _context.ProcedureRequest.CountAsync(r => r.IdStatus == 3),
                Cancelados = await _context.ProcedureRequest.CountAsync(r => r.IdStatus == 5)
            };

            ViewBag.Stats = stats;

            var recientes = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureStatus)
                .OrderByDescending(r => r.DateUpdated)
                .Take(5)
                .ToListAsync();

            return View(recientes);
        }
    }
}
