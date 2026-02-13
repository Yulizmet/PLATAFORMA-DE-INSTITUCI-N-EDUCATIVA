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

        [Area("UserMng")]
        public IActionResult StudentCrud()
        {
            return View();
        }
        
        [Area("UserMng")]
        public IActionResult TeacherCrud()
        {
            return View();
        }
        
        [Area("UserMng")]
        public IActionResult Test()
        {
            return View();
        }
    }
}
