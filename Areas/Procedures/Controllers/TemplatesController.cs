using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class TemplatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
