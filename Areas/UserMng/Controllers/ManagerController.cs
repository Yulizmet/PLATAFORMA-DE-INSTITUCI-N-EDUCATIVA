using Azure;
using Microsoft.AspNetCore.Mvc;
using SchoolManager.Data;

namespace SchoolManager.Areas.UserMng.Controllers
{
    public class ManagerController : Controller
    {
        private readonly AppDbContext _context;

        public ManagerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }
    }
}
