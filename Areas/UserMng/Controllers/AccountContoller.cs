using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SchoolManager.Areas.UserMng.ViewModels;
using SchoolManager.Data;
using System.Security.Claims;

namespace SchoolManager.Areas.UserMng.Controllers

{

    [Area("UserMng")]

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

                return RedirectToAction("Index", "Home");

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

                ModelState.AddModelError(string.Empty, $"Tu cuenta está bloqueada. Razón: {usuario.LockReason}");

                return View(model);

            }


            var claims = new List<Claim>

        {

            new Claim(ClaimTypes.Name,  usuario.Username),

            new Claim(ClaimTypes.Email, usuario.Email),

            new Claim("UserId",         usuario.UserId.ToString()),

            new Claim("PersonId",       usuario.PersonId.ToString()),

        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

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

            return RedirectToAction("Index", "Home");

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

    }



}
