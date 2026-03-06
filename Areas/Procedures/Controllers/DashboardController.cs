using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.ViewModels;
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

        public async Task<IActionResult> Index(int? year)
        {
            int selectedYear = year ?? DateTime.Now.Year;

            var solicitudesBase = _context.ProcedureRequest
                .Include(r => r.ProcedureFlow)
                    .ThenInclude(f => f.ProcedureStatus)
                .Where(r => r.DateCreated.Year == selectedYear);

            var vm = new DashboardViewModel
            {
                SelectedYear = selectedYear,
                TotalRequests = await solicitudesBase.CountAsync(),
                ActionRequired = await solicitudesBase.CountAsync(r => r.ProcedureFlow.ProcedureStatus.IsActionRequiredByUser),
                Done = await solicitudesBase.CountAsync(r => r.ProcedureFlow.ProcedureStatus.IsTerminalState),
                InProgress = await solicitudesBase.CountAsync(r =>
                    !r.ProcedureFlow.ProcedureStatus.IsTerminalState &&
                    !r.ProcedureFlow.ProcedureStatus.IsActionRequiredByUser),
                Cancelled = await solicitudesBase.CountAsync(r => r.ProcedureFlow.ProcedureStatus.Name == "Cancelado")
            };

            var closedStats = await solicitudesBase
                .Where(r => r.DateTerminated != null)
                .Select(r => EF.Functions.DateDiffMinute(r.DateCreated, r.DateTerminated!.Value))
                .ToListAsync();

            double avgClosedMin = closedStats.Any() ? closedStats.Average() : 0;
            TimeSpan tsClosed = TimeSpan.FromMinutes(avgClosedMin);
            vm.AvgResolutionTime = $"{(int)tsClosed.TotalDays}d {tsClosed.Hours}h {tsClosed.Minutes}m";
            vm.AvgResolutionHours = tsClosed.TotalHours;

            var openStats = await solicitudesBase
                .Where(r => r.DateTerminated == null)
                .Select(r => EF.Functions.DateDiffMinute(r.DateCreated, DateTime.Now))
                .ToListAsync();

            double avgOpenMin = openStats.Any() ? openStats.Average() : 0;
            TimeSpan tsOpen = TimeSpan.FromMinutes(avgOpenMin);
            vm.AvgWaitTime = $"{(int)tsOpen.TotalDays}d {tsOpen.Hours}h {tsOpen.Minutes}m";
            vm.AvgWaitHours = tsOpen.TotalHours;

            var solicitudesMes = await solicitudesBase
                .GroupBy(r => r.DateCreated.Month)
                .Select(g => new { Mes = g.Key, Total = g.Count() })
                .ToListAsync();

            vm.MonthlyRequests = new int[12];
            foreach (var item in solicitudesMes)
            {
                vm.MonthlyRequests[item.Mes - 1] = item.Total;
            }

            vm.RequestsByArea = await solicitudesBase
                .GroupBy(r => r.ProcedureType.ProcedureArea.Name ?? "Sin Área")
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Name, x => x.Count);

            vm.RequestsByType = await solicitudesBase
                .GroupBy(r => r.ProcedureType.Name)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Name, x => x.Count);

            return View(vm);
        }
    }
}