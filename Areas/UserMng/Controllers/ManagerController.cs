using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManager.Areas.UserMng.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;
using Microsoft.EntityFrameworkCore;

namespace SchoolManager.Areas.UserMng.Controllers;

[Area("UserMng")]
[Authorize(Roles = "Administrator,Head Nurse,Head of Psychology, Master")]
public class ManagerController : Controller
{
    private readonly AppDbContext _context;

    private static readonly HashSet<string> _excludedRoles = new() { "Student", "Teacher" };

    private static readonly Dictionary<string, (string DisplayName, string Icon, string Color)> _roleDisplay = new()
    {
        ["Student"]            = ("Alumnos",              "bi-mortarboard-fill",       "primary"),
        ["Teacher"]            = ("Maestros",             "bi-briefcase-fill",         "success"),
        ["Administrator"]      = ("Administradores",      "bi-shield-lock-fill",       "dark"),
        ["Nurse"]              = ("Enfermeras",           "bi-heart-pulse-fill",       "danger"),
        ["Psychologist"]       = ("Psicólogos",           "bi-bandaid-fill",                  "info"),
        ["Head Nurse"]         = ("Jefes de Enfermería",  "bi-hospital-fill",          "warning"),
        ["Head of Psychology"] = ("Jefes de Psicología",  "bi-clipboard2-pulse-fill",  "secondary"),
        ["Coordinator"]        = ("Coordiadores",    "bi-star-fill",     "dark"),
        ["Master"]             = ("Masters",              "bi-star-fill",              "dark"),
    };

    // roles puede gestionar cada quien
    private static readonly Dictionary<string, HashSet<string>> _rolePermissions = new()
    {
        ["Administrator"]      = new() { "Administrator", "Nurse", "Psychologist", "Head Nurse", "Head of Psychology", "Master", "Coordinator" },
        ["Head Nurse"]         = new() { "Nurse" },
        ["Head of Psychology"] = new() { "Psychologist" },
        ["Master"] = new() { "Administrator", "Nurse", "Psychologist", "Head Nurse", "Head of Psychology" , "Master"},
    };

    public ManagerController(AppDbContext context)
    {
        _context = context;
    }

    // qué roles puede gestionar el usuario actual
    private HashSet<string> GetManageableRoles()
    {
        var manageable = new HashSet<string>();

        foreach (var (roleName, allowedRoles) in _rolePermissions)
        {
            if (User.IsInRole(roleName))
            {
                foreach (var r in allowedRoles)
                    manageable.Add(r);
            }
        }

        return manageable;
    }
    
    private bool CanManageRole(string role)
    {
        return GetManageableRoles().Contains(role);
    }
    
    public async Task<IActionResult> Index()
    {
        var manageableRoles = GetManageableRoles();
        var isAdmin = User.IsInRole("Administrator");

        var roles = await _context.Roles
            .Where(r => r.IsActive)
            .ToListAsync();

        var roleCounts = await _context.UserRoles
            .Where(ur => ur.IsActive)
            .Join(_context.Users.Where(u => u.IsActive),
                ur => ur.UserId, u => u.UserId, (ur, u) => ur.RoleId)
            .GroupBy(roleId => roleId)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Count);

        var filteredRoles = roles.Where(r =>
            isAdmin || // Admin ve
            manageableRoles.Contains(r.Name) ||
            (isAdmin && (r.Name == "Student" || r.Name == "Teacher"))
        ).ToList();

        if (isAdmin)
            filteredRoles = roles;

        var vm = new DashboardVM
        {
            TotalActiveUsers = isAdmin
                ? await _context.Users.CountAsync(u => u.IsActive)
                : await _context.UserRoles
                    .Where(ur => ur.IsActive)
                    .Join(_context.Users.Where(u => u.IsActive),
                        ur => ur.UserId, u => u.UserId, (ur, u) => new { ur.RoleId, u.UserId })
                    .Join(_context.Roles.Where(r => manageableRoles.Contains(r.Name)),
                        x => x.RoleId, r => r.RoleId, (x, r) => x.UserId)
                    .Distinct()
                    .CountAsync(),

            TotalActivePersons = isAdmin
                ? await _context.Persons.CountAsync(p => p.IsActive)
                : 0,

            Roles = filteredRoles.Select(r =>
            {
                var (displayName, icon, color) = _roleDisplay
                    .GetValueOrDefault(r.Name, (r.Name, "bi-person", "secondary"));

                roleCounts.TryGetValue(r.RoleId, out int count);

                string url;
                if (r.Name == "Student")
                    url = Url.Action("Index", "Students", new { area = "UserMng" })!;
                else if (r.Name == "Teacher")
                    url = Url.Action("Index", "Teachers", new { area = "UserMng" })!;
                else
                    url = Url.Action("Users", "Manager", new { area = "UserMng", role = r.Name })!;

                return new RoleCardVM
                {
                    Name = r.Name,
                    DisplayName = displayName,
                    Description = r.Description,
                    ActiveCount = count,
                    Icon = icon,
                    Color = color,
                    Url = url
                };
            }).ToList()
        };

        return View(vm);
    }

    public async Task<IActionResult> Users(string role)
    {
        if (string.IsNullOrEmpty(role))
            return RedirectToAction("Index");

        if (role == "Student")
            return RedirectToAction("Index", "Students", new { area = "UserMng" });
        if (role == "Teacher")
            return RedirectToAction("Index", "Teachers", new { area = "UserMng" });

        if (!CanManageRole(role))
        {
            return RedirectToAction("Index");
        }

        var roleExists = await _context.Roles.AnyAsync(r => r.Name == role && r.IsActive);
        if (!roleExists)
            return RedirectToAction("Index");

        var users = await _context.Users
            .Include(u => u.Person)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.IsActive
                        && u.UserRoles.Any(ur => ur.Role.Name == role && ur.IsActive))
            .ToListAsync();

        var (displayName, icon, color) = _roleDisplay
            .GetValueOrDefault(role, (role, "bi-person", "secondary"));

        ViewBag.RoleName = role;
        ViewBag.RoleDisplayName = displayName;
        ViewBag.RoleIcon = icon;
        ViewBag.RoleColor = color;

        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Person)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
            return Json(new { success = false, errors = new[] { "Usuario no encontrado." } });

        // verificar que puede gestionar al menos uno de los roles del usuario
        var userRoles = user.UserRoles.Where(ur => ur.IsActive).Select(ur => ur.Role.Name);
        var manageable = GetManageableRoles();
        if (!userRoles.Any(r => manageable.Contains(r)))
            return Json(new { success = false, errors = new[] { "No tienes permiso para ver este usuario." } });

        return Json(new
        {
            success = true,
            data = new
            {
                userId = user.UserId,
                firstName = user.Person.FirstName,
                lastNamePaternal = user.Person.LastNamePaternal,
                lastNameMaternal = user.Person.LastNameMaternal,
                birthDate = user.Person.BirthDate?.ToString("yyyy-MM-dd"),
                gender = user.Person.Gender,
                curp = user.Person.Curp,
                email = user.Email,
                phone = user.Person.Phone,
                username = user.Username
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStaffVM model)
    {
        if (!CanManageRole(model.RoleName))
            return Json(new { success = false, errors = new[] { "No tienes permiso para crear usuarios con este rol." } });

        if (_excludedRoles.Contains(model.RoleName))
            return Json(new { success = false, errors = new[] { "Usa el controlador específico para este rol." } });

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errors });
        }

        var roleEntity = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == model.RoleName && r.IsActive);

        if (roleEntity == null)
            return Json(new { success = false, errors = new[] { "Rol no válido." } });

        if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.IsActive))
            return Json(new { success = false, errors = new[] { "Este correo ya está registrado." } });

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var person = new users_person
            {
                FirstName = model.FirstName,
                LastNamePaternal = model.LastNamePaternal,
                LastNameMaternal = model.LastNameMaternal ?? "",
                BirthDate = model.BirthDate,
                Gender = model.Gender ?? "",
                Curp = model.Curp ?? "",
                Email = model.Email ?? "",
                Phone = model.Phone ?? "",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Persons.Add(person);
            await _context.SaveChangesAsync();

            var user = new users_user
            {
                PersonId = person.PersonId,
                Username = model.Username,
                Email = model.Email ?? "",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                IsLocked = false,
                LockReason = "",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new users_userrole
            {
                UserId = user.UserId,
                RoleId = roleEntity.RoleId,
                CreatedDate = DateTime.Now,
                IsActive = true
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { success = false, errors = new[] { "Error al guardar: " + ex.Message } });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditStaffVM model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errors });
        }

        try
        {
            var user = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == model.UserId);

            if (user == null)
                return Json(new { success = false, errors = new[] { "Usuario no encontrado." } });

            var userRoles = user.UserRoles.Where(ur => ur.IsActive).Select(ur => ur.Role.Name);
            if (!userRoles.Any(r => CanManageRole(r)))
                return Json(new { success = false, errors = new[] { "No tienes permiso para editar este usuario." } });

            user.Person.FirstName = model.FirstName;
            user.Person.LastNamePaternal = model.LastNamePaternal;
            user.Person.LastNameMaternal = model.LastNameMaternal ?? "";
            user.Person.BirthDate = model.BirthDate;
            user.Person.Gender = model.Gender ?? "";
            user.Person.Curp = model.Curp ?? "";
            user.Person.Email = model.Email ?? "";
            user.Person.Phone = model.Phone ?? "";

            user.Username = model.Username;
            user.Email = model.Email ?? "";

            if (!string.IsNullOrWhiteSpace(model.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, errors = new[] { "Error al actualizar: " + ex.Message } });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return Json(new { success = false, errors = new[] { "Usuario no encontrado." } });
            
            var userRoles = user.UserRoles.Where(ur => ur.IsActive).Select(ur => ur.Role.Name);
            if (!userRoles.Any(r => CanManageRole(r)))
                return Json(new { success = false, errors = new[] { "No tienes permiso para desactivar este usuario." } });

            user.IsActive = false;
            user.Person.IsActive = false;

            foreach (var ur in user.UserRoles)
                ur.IsActive = false;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, errors = new[] { "Error al desactivar: " + ex.Message } });
        }
    }
}