using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}