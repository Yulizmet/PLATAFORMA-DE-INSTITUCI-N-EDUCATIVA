using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.MainScreen.Controllers
{
    [Area("MainScreen")]
    public class MainScreenController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult View2() {
            return View();
        }
    }
}
