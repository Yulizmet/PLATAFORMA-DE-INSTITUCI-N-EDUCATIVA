using Microsoft.AspNetCore.Mvc;
using SchoolManager.Areas.UserMng.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;
using Microsoft.EntityFrameworkCore;

namespace SchoolManager.Areas.UserMng.Controllers;

[Area("UserMng")]
public class TeachersController : Controller
{
    private readonly AppDbContext _context;

    public TeachersController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var teachers = await _context.Users
            .Include(u => u.Person)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.IsActive 
                        && u.UserRoles.Any(ur => ur.Role.Name == "Teacher"))
            .ToListAsync();

        return View(teachers);
    }

    [HttpGet]
    public async Task<IActionResult> GetTeacher(int id)
    {
        var user = await _context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
            return Json(new { success = false, errors = new[] { "Maestro no encontrado." } });

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
    public async Task<IActionResult> Create(CreateTeacherVM model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var person = new users_person
            {
                FirstName = model.FirstName,
                LastNamePaternal = model.LastNamePaternal,
                LastNameMaternal = model.LastNameMaternal,
                BirthDate = model.BirthDate,
                Gender = model.Gender,
                Curp = model.Curp,
                Email = model.Email,
                Phone = model.Phone,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Persons.Add(person);
            await _context.SaveChangesAsync();

            var user = new users_user
            {
                PersonId = person.PersonId,
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                IsLocked = false,
                LockReason = "",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var teacherRoleId = await _context.Roles
                .Where(r => r.Name == "Teacher")
                .Select(r => r.RoleId)
                .FirstAsync();

            _context.UserRoles.Add(new users_userrole
            {
                UserId = user.UserId,
                RoleId = teacherRoleId,
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
    public async Task<IActionResult> Edit(EditTeacherVM model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == model.UserId);

            if (user == null)
                return Json(new { success = false, errors = new[] { "Maestro no encontrado." } });

            // Actualizar persona
            user.Person.FirstName = model.FirstName;
            user.Person.LastNamePaternal = model.LastNamePaternal;
            user.Person.LastNameMaternal = model.LastNameMaternal;
            user.Person.BirthDate = model.BirthDate;
            user.Person.Gender = model.Gender;
            user.Person.Curp = model.Curp;
            user.Person.Email = model.Email;
            user.Person.Phone = model.Phone;

            user.Username = model.Username;
            user.Email = model.Email;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = HashPassword(model.Password);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { success = false, errors = new[] { "Error al actualizar: " + ex.Message } });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return Json(new { success = false, errors = new[] { "Maestro no encontrado." } });

            _context.UserRoles.RemoveRange(user.UserRoles);

            _context.Users.Remove(user);

            _context.Persons.Remove(user.Person);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { success = false, errors = new[] { "Error al eliminar: " + ex.Message } });
        }
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}