using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.UserMng.Controllers
{
    [Area("UserMng")]
    [AllowAnonymous]
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

            ViewBag.Careers = await _context.PreenrollmentCareers
                .Where(c => c.IsActive)
                .OrderBy(c => c.name_career)
                .ToListAsync();

            ViewBag.Generations = await _context.PreenrollmentGenerations
                .OrderByDescending(g => g.Year)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> QuickUser(
            string firstName,
            string roleName,
            string password = "Test1234!",
            int? careerId = null,
            int? generationId = null)
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

            string? matricula = null;
            if (roleName == "Student")
            {
                matricula = await CreateStudentPreenrollment(
                    user.UserId, person.PersonId, careerId, generationId);
            }

            var result = $"Usuario creado — Email: {user.Email} | Password: {password} | Rol: {roleName}";
            if (matricula != null)
                result += $" | Matrícula: {matricula}";

            TempData["Result"] = result;
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

        private async Task<string?> CreateStudentPreenrollment(
            int userId, int personId, int? careerId, int? generationId)
        {
            var career = careerId.HasValue
                ? await _context.PreenrollmentCareers.FindAsync(careerId.Value)
                : await _context.PreenrollmentCareers.FirstOrDefaultAsync(c => c.IsActive);

            if (career == null) return null;

            var generation = generationId.HasValue
                ? await _context.PreenrollmentGenerations.FindAsync(generationId.Value)
                : await _context.PreenrollmentGenerations
                    .OrderByDescending(g => g.Year)
                    .FirstOrDefaultAsync();

            if (generation == null) return null;

            var matricula = await GenerateMatricula(generation.Year);

            var preenrollment = new preenrollment_general
            {
                UserId = userId,
                PersonId = personId,
                IdCareer = career.IdCareer,
                IdGeneration = generation.IdGeneration,
                Matricula = matricula,
                MaritalStatus = "Soltero",
                Nationality = "Mexicana",
                Work = false,
                CreateStat = DateTime.Now,
                Folio = $"DEV-{DateTime.Now.Ticks.ToString()[^6..]}"
            };

            _context.PreenrollmentGenerals.Add(preenrollment);
            await _context.SaveChangesAsync();

            return matricula;
        }

        private async Task<string> GenerateMatricula(int year)
        {
            var prefix = year.ToString();
            var count = await _context.PreenrollmentGenerals
                .CountAsync(p => p.Matricula != null
                              && p.Matricula.StartsWith(prefix));

            return $"{prefix}{(count + 1):D4}";
        }
    }
}