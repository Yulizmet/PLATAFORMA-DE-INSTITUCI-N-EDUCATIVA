using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.ViewModels;
using SchoolManager.Data;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class DashboardController : _ProceduresBaseController
    {
        public DashboardController(AppDbContext context) : base(context) { }

        [AllowAnonymous]
        public IActionResult DeniedAccessPage()
        {
            Response.StatusCode = 403;
            return View();
        }

        public async Task<IActionResult> Index(int? year, int? areaId)
        {
            await LoadPermissions("Panel de control");
            int selectedYear = year ?? DateTime.Now.Year;

            ViewBag.Areas = await _context.ProcedureAreas.OrderBy(a => a.Name).ToListAsync();
            ViewBag.SelectedArea = areaId;

            var solicitudesBase = _context.ProcedureRequest
                .Include(r => r.ProcedureType).ThenInclude(pt => pt.ProcedureArea)
                .Include(r => r.ProcedureFlow).ThenInclude(f => f.ProcedureStatus)
                .Where(r => r.DateCreated.Year == selectedYear);

            var solicitudesFiltradas = solicitudesBase;
            if (areaId.HasValue && areaId > 0)
            {
                solicitudesFiltradas = solicitudesBase.Where(r => r.ProcedureType.IdArea == areaId);
            }

            var vm = new DashboardViewModel
            {
                SelectedYear = selectedYear,
                TotalRequests = await solicitudesFiltradas.CountAsync(),
                ActionRequired = await solicitudesFiltradas.CountAsync(r => r.ProcedureFlow.ProcedureStatus.IsActionRequiredByUser),
                Done = await solicitudesFiltradas.CountAsync(r => r.ProcedureFlow.ProcedureStatus.IsTerminalState),
                InProgress = await solicitudesFiltradas.CountAsync(r =>
                    !r.ProcedureFlow.ProcedureStatus.IsTerminalState &&
                    !r.ProcedureFlow.ProcedureStatus.IsActionRequiredByUser),
                Cancelled = await solicitudesFiltradas.CountAsync(r => r.ProcedureFlow.ProcedureStatus.Name == "Cancelado")
            };

            var closedStats = await solicitudesFiltradas
                .Where(r => r.DateTerminated != null)
                .Select(r => EF.Functions.DateDiffMinute(r.DateCreated, r.DateTerminated!.Value))
                .ToListAsync();

            double avgClosedMin = closedStats.Any() ? closedStats.Average() : 0;
            TimeSpan tsClosed = TimeSpan.FromMinutes(avgClosedMin);
            vm.AvgResolutionTime = $"{(int)tsClosed.TotalDays}d {tsClosed.Hours}h {tsClosed.Minutes}m";
            vm.AvgResolutionHours = tsClosed.TotalHours;

            var openStats = await solicitudesFiltradas
                .Where(r => r.DateTerminated == null)
                .Select(r => EF.Functions.DateDiffMinute(r.DateCreated, DateTime.Now))
                .ToListAsync();

            double avgOpenMin = openStats.Any() ? openStats.Average() : 0;
            TimeSpan tsOpen = TimeSpan.FromMinutes(avgOpenMin);
            vm.AvgWaitTime = $"{(int)tsOpen.TotalDays}d {tsOpen.Hours}h {tsOpen.Minutes}m";
            vm.AvgWaitHours = tsOpen.TotalHours;

            var solicitudesMes = await solicitudesFiltradas
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

            vm.RequestsByType = await solicitudesFiltradas
                .GroupBy(r => r.ProcedureType.Name)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Name, x => x.Count);

            return View(vm);
        }
    }
}