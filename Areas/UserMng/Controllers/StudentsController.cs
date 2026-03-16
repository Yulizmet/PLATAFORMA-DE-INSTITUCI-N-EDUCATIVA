using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManager.Areas.UserMng.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;
using Microsoft.EntityFrameworkCore;

namespace SchoolManager.Areas.UserMng.Controllers;

[Area("UserMng")]
[Authorize(Roles = "Administrator")]
public class StudentsController : Controller
{
    private readonly AppDbContext _context;

    public StudentsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var students = await _context.Users
            .Include(u => u.Person)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Preenrollments)
            .Where(u => u.IsActive
                        && u.UserRoles.Any(ur => ur.Role.Name == "Student"))
            .ToListAsync();

        return View(students);
    }

    [HttpGet]
    public async Task<IActionResult> GetStudent(int id)
    {
        var user = await _context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
            return Json(new { success = false, errors = new[] { "Alumno no encontrado." } });

        return Json(new
        {
            success = true,
            data = new
            {
                userId = user.UserId,
                email = user.Email,
                username = user.Username
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStudentVM model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
        }

        var person = await _context.Persons.FindAsync(model.PersonId);
        if (person == null)
            return Json(new { success = false, errors = new[] { "Persona no encontrada." } });

        var existingUser = await _context.Users
            .AnyAsync(u => u.PersonId == model.PersonId);
        if (existingUser)
            return Json(new { success = false, errors = new[] { "Esta persona ya tiene una cuenta." } });

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == model.Email);
        if (emailExists)
            return Json(new { success = false, errors = new[] { "Este correo ya está registrado." } });

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new users_user
            {
                PersonId = model.PersonId,
                Username = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                IsLocked = false,
                LockReason = "",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var studentRoleId = await _context.Roles
                .Where(r => r.Name == "Student")
                .Select(r => r.RoleId)
                .FirstAsync();

            _context.UserRoles.Add(new users_userrole
            {
                UserId = user.UserId,
                RoleId = studentRoleId,
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
    public async Task<IActionResult> Edit(EditStudentVM model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
        }

        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == model.UserId);

            if (user == null)
                return Json(new { success = false, errors = new[] { "Alumno no encontrado." } });

            user.Username = model.Username;
            user.Email = model.Email;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }

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
    public async Task<IActionResult> Delete(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return Json(new { success = false, errors = new[] { "Alumno no encontrado." } });

            _context.UserRoles.RemoveRange(user.UserRoles);
            _context.Users.Remove(user);

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
}