using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.UserMng.ViewModels;
using SchoolManager.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SchoolManager.Areas.UserMng.Controllers
{
    [Area("UserMng")]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectByRole(User);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = _context.Users
                .FirstOrDefault(u => u.Email == model.Email && u.IsActive == true);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                return View(model);
            }

            if (usuario.IsLocked)
            {
                ModelState.AddModelError(string.Empty, 
                    $"Tu cuenta está bloqueada. Razón: {usuario.LockReason}");
                return View(model);
            }

            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == usuario.UserId && ur.IsActive)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.RoleId,
                    (ur, r) => r)
                .Where(r => r.IsActive)
                .ToListAsync();

            if (!userRoles.Any())
            {
                ModelState.AddModelError(string.Empty, 
                    "No tienes ningún rol asignado. Contacta al administrador.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,  usuario.Username),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("UserId",         usuario.UserId.ToString()),
                new Claim("PersonId",       usuario.PersonId.ToString()),
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProps
            );

            usuario.LastLoginDate = DateTime.Now;
            _context.SaveChanges();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectByRole(principal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account", new { area = "UserMng" });
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        
        private IActionResult RedirectByRole(ClaimsPrincipal principal)
        {

            if (principal.IsInRole("Administrator"))
                return RedirectToAction("Index", "Manager", new { area = "UserMng" });
            
            if (principal.IsInRole("Master"))
                return RedirectToAction("Index", "Manager", new { area = "UserMng" });

            if (principal.IsInRole("Head Nurse"))
                return RedirectToAction("Users", "Manager", new { area = "UserMng", role = "Nurse" });

            if (principal.IsInRole("Head of Psychology"))
                return RedirectToAction("Users", "Manager", new { area = "UserMng", role = "Psychologist" });

            if (principal.IsInRole("Teacher"))
                return RedirectToAction("Index", "Teachers", new { area = "UserMng" });

            if (principal.IsInRole("Nurse"))
                return RedirectToAction("Index", "Home"); // RAMOS actualiza cuando tengas tu vista

            if (principal.IsInRole("Psychologist"))
                return RedirectToAction("Index", "Home"); // RAMOS actualiza cuando tengas tu vista

            if (principal.IsInRole("Student"))
                return RedirectToAction("SistemaEscolar", "MainScreen", new { area = "MainScreen" });

            return RedirectToAction("Index", "MainScreen", new { area = "MainScreen" });
        }
    }
}