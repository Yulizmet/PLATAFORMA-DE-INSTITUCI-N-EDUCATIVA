using Microsoft.AspNetCore.Mvc;

namespace SchoolManager.Areas.Procedures.Controllers
{
    public class ProcedureManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
