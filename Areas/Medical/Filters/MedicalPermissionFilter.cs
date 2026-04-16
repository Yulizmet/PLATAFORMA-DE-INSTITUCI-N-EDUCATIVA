using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;

namespace SchoolManager.Areas.Medical.Filters
{
    public class MedicalPermissionFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _context;

        public MedicalPermissionFilter(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            var controladoresProtegidos = new[] { "Logbook", "Psychology" };

            if (!controladoresProtegidos.Contains(controller))
            {
                await next();
                return;
            }

            if (context.HttpContext.User.IsInRole("Master") ||
                context.HttpContext.User.IsInRole("Coordinator") ||
                context.HttpContext.User.IsInRole("Head Nurse") ||
                context.HttpContext.User.IsInRole("Head of Psychology"))
            {
                await next();
                return;
            }

            var staffClaim = context.HttpContext.User.FindFirst("StaffId");
            if (staffClaim == null)
            {
                context.Result = new ViewResult { ViewName = "AccesoDenegado" };
                return;
            }

            var staffId = int.Parse(staffClaim.Value);

            var permisos = await _context.MedicalPermissions
                .FirstOrDefaultAsync(p => p.StaffId == staffId);

            if (permisos == null)
            {
                context.Result = new ViewResult { ViewName = "AccesoDenegado" };
                return;
            }

            bool tieneAcceso = action switch
            {
                "Index" or "Details" => permisos.Ver,
                "Create" => permisos.Agregar,
                "Edit" => permisos.Modificar,
                "Delete" or "DeleteConfirmed" => permisos.Borrar,
                _ => true
            };

            var staffRoleClaim = context.HttpContext.User.FindFirst("StaffRoleId");
            if (staffRoleClaim != null)
            {
                var roleId = int.Parse(staffRoleClaim.Value);

                if (controller == "Logbook" && roleId == 19) tieneAcceso = false;
                if (controller == "Psychology" && roleId == 18) tieneAcceso = false;
            }

            if (!tieneAcceso)
            {
                context.Result = new ViewResult { ViewName = "AccesoDenegado" };
                return;
            }

            await next();
        }
    }
}