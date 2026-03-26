using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.Filters;
using SchoolManager.Data;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    [Authorize(Roles = "Administrator")]
    [ServiceFilter(typeof(ProcedureRouteAuthorizeAttribute))]
    public abstract class _ProceduresBaseController : Controller
    {
        protected readonly AppDbContext _context;

        protected _ProceduresBaseController(AppDbContext context)
        {
            _context = context;
        }

        protected async Task LoadPermissions(string moduleName)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim == null) return;
            int userId = int.Parse(userIdClaim);

            var staff = await _context.ProcedureStaff
                .FirstOrDefaultAsync(s => s.IdUser == userId && s.IsActive);

            if (staff != null)
            {
                ViewBag.AllowedModules = await _context.ProcedurePermissions
                    .Include(p => p.ModuleCatalog)
                    .Where(p => p.IdArea == staff.IdArea &&
                                p.IdJobPosition == staff.IdJobPosition &&
                                p.CanView)
                    .Select(p => p.ModuleCatalog!.ModuleName)
                    .Distinct()
                    .ToListAsync();

                ViewBag.UserPermissions = await _context.ProcedurePermissions
                    .Include(p => p.ModuleCatalog)
                    .Where(p => p.IdArea == staff.IdArea &&
                                p.IdJobPosition == staff.IdJobPosition &&
                                p.CanView &&
                                p.ModuleCatalog!.ModuleName == moduleName)
                    .Select(p => p.ModuleCatalog!.ButtonName)
                    .ToListAsync();
            }
        }
    }
}