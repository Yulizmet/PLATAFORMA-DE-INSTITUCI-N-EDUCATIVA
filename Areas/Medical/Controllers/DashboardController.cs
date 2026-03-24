using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Nurse, Psychologist, Head Nurse, Head of Psychology, Coordinator, Master")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // TEMPORAL - quitar después
            var roles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            ViewBag.Roles = string.Join(", ", roles);

            return View();
        }
    }
}