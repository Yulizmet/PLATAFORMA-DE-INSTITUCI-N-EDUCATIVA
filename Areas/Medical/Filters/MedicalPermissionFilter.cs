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

            // Verificar que el usuario tenga StaffId en sus claims
            var staffClaim = context.HttpContext.User.FindFirst("StaffId");
            if (staffClaim == null)
            {
                context.Result = new ViewResult { ViewName = "AccesoDenegado" };
                return;
            }

            var staffId = int.Parse(staffClaim.Value);

            // Consultar permisos en BD
            var permisos = await _context.MedicalPermissions
                .FirstOrDefaultAsync(p => p.StaffId == staffId);

            if (permisos == null)
            {
                context.Result = new ViewResult { ViewName = "AccesoDenegado" };
                return;
            }

            // Verificar permiso según el action
            bool tieneAcceso = action switch
            {
                "Index" or "Details" => permisos.Ver,
                "Create" => permisos.Agregar,
                "Edit" => permisos.Modificar,
                "Delete" or "DeleteConfirmed" => permisos.Borrar,
                _ => true
            };

            var rolesSuperior = new[] { 6, 20, 21, 22 };

            var staffRoleClaim = context.HttpContext.User.FindFirst("StaffRoleId");
            if (staffRoleClaim != null)
            {
                var roleId = int.Parse(staffRoleClaim.Value);

                if (rolesSuperior.Contains(roleId))
                {
                    await next();
                    return;
                }

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