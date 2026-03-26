using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using System.Security.Claims;

namespace SchoolManager.Areas.Procedures.Filters
{
    public class ProcedureRouteAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly AppDbContext _context;

        public ProcedureRouteAuthorizeAttribute(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            if (user.IsInRole("Student"))
            {
                await next();
                return;
            }

            var userIdClaim = user.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "UserMng" });
                return;
            }

            int userId = int.Parse(userIdClaim);

            var controller = context.RouteData.Values["controller"]?.ToString();
            var area = context.RouteData.Values["area"]?.ToString();
            string currentRoute = $"/{area}/{controller}";

            if (controller == "Dashboard")
            {
                await next();
                return;
            }

            var staff = await _context.ProcedureStaff
                .FirstOrDefaultAsync(s => s.IdUser == userId && s.IsActive);

            if (staff == null)
            {
                context.Result = new RedirectToActionResult("DeniedAccessPage", "Dashboard", new { area = "Procedures" });
                return;
            }

            var hasPermission = await _context.ProcedurePermissions
                .Include(p => p.ModuleCatalog)
                .AnyAsync(p => p.IdArea == staff.IdArea &&
                               p.IdJobPosition == staff.IdJobPosition &&
                               p.ModuleCatalog!.Route!.StartsWith(currentRoute) &&
                               p.CanView);

            if (!hasPermission)
            {
                context.Result = new RedirectToActionResult("DeniedAccessPage", "Dashboard", new { area = "Procedures" });
                return;
            }

            await next();
        }
    }
}