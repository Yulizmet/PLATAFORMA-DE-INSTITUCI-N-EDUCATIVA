using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Areas.Procedures.ViewModels;

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
            var solicitudesBase = _context.ProcedureRequest
                .Include(r => r.ProcedureFlow)
                    .ThenInclude(f => f.ProcedureStatus);

            var vm = new DashboardViewModel
            {
                TotalRequests = await _context.ProcedureRequest.CountAsync(),
                ActionRequired = await solicitudesBase.CountAsync(r => r.ProcedureFlow.ProcedureStatus.IsActionRequiredByUser),
                Done = await solicitudesBase.CountAsync(r => r.ProcedureFlow.ProcedureStatus.IsTerminalState),
                InProgress = await solicitudesBase.CountAsync(r =>
                    !r.ProcedureFlow.ProcedureStatus.IsTerminalState &&
                    !r.ProcedureFlow.ProcedureStatus.IsActionRequiredByUser),

                Cancelled = 0
            };

            vm.Recientes = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureFlow)
                    .ThenInclude(f => f.ProcedureStatus)
                .OrderByDescending(r => r.DateUpdated)
                .Take(5)
                .ToListAsync();

            return View(vm);
        }
    }
}
