using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.SocialService.Controllers
{
    [Area("SocialService")]
    public class AccountController : Controller
    {
        // GET: Acceso (selección de dashboard)
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // CERRAR SESIÓN (SOLO REDIRIGE)
        [HttpGet]
        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Account");
        }
    }
}
