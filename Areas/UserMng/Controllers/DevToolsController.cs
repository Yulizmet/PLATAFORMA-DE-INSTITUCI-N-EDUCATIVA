using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.UserMng.Controllers
{
    [Area("UserMng")]
    public class DevToolsController : Controller
    {
        private readonly AppDbContext _context;

        public DevToolsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();

            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> QuickUser(string firstName, string roleName, string password = "Test1234!")
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                return BadRequest($"Rol '{roleName}' no encontrado.");

            var timestamp = DateTime.Now.Ticks.ToString()[^4..];

            var person = new users_person
            {
                FirstName = firstName,
                LastNamePaternal = "Test",
                LastNameMaternal = "Dev",
                Gender = "M",
                Curp = $"TEST{timestamp}XXXXXX00",
                Email = $"{firstName.ToLower()}{timestamp}@test.dev",
                Phone = "0000000000",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.Persons.Add(person);
            await _context.SaveChangesAsync();

            var user = new users_user
            {
                PersonId = person.PersonId,
                Username = $"{firstName.ToLower()}{timestamp}",
                Email = person.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsLocked = false,
                LockReason = "",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new users_userrole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                IsActive = true,
                CreatedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["Result"] = $"Usuario creado — Email: {user.Email} | Password: {password} | Rol: {roleName}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["RoleError"] = "El nombre del rol es obligatorio.";
                return RedirectToAction("Index");
            }

            var exists = await _context.Roles
                .AnyAsync(r => r.Name.ToLower() == name.Trim().ToLower());

            if (exists)
            {
                TempData["RoleError"] = $"Ya existe un rol con el nombre '{name}'.";
                return RedirectToAction("Index");
            }

            var role = new users_role
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? "",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            TempData["RoleResult"] = $"Rol creado — Nombre: {role.Name} | ID: {role.RoleId}";
            return RedirectToAction("Index");
        }
    }
}