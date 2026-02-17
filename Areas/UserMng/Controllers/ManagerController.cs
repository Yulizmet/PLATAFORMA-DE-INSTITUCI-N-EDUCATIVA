using Azure;
using Microsoft.AspNetCore.Mvc;
using SchoolManager.Data;

namespace SchoolManager.Areas.UserMng.Controllers
{
    [Area("UserMng")]
    public class ManagerController : Controller
    {
        private readonly AppDbContext _context;

        public ManagerController(AppDbContext context)
        {
            _context = context;
        }

        
        public IActionResult StudentCrud()
        {
            return View();
        }
        public IActionResult TeacherCrud()
        {
            return View();
        }
        
        public IActionResult Test()
        {
            return View();
        }
    }
}
