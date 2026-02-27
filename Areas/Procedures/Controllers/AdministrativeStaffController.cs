using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class AdministrativeStaffController : Controller
    {

        private readonly AppDbContext _context;

        public AdministrativeStaffController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Areas = await _context.ProcedureAreas.OrderBy(a => a.Name).ToListAsync();

            var staff = await _context.ProcedureStaff
                .Include(s => s.User).ThenInclude(u => u.Person)
                .Include(s => s.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(s => s.ProcedureArea)
                .Select(s => new StaffViewModel
                {
                    Id = s.Id,
                    UserId = s.IdUser,
                    FullName = $"{s.User.Person.FirstName} {s.User.Person.LastNamePaternal} {s.User.Person.LastNameMaternal}",
                    Username = s.User.Username,
                    Email = s.User.Email,
                    JobPosition = s.JobPosition,
                    AreaName = s.ProcedureArea.Name,
                    IsSuperAdmin = s.IsSuperAdmin,
                    IsActive = s.IsActive,
                    Roles = s.User.UserRoles.Select(ur => ur.Role.Name).ToList()
                }).ToListAsync();

            return View(staff);
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var staffUserIds = await _context.ProcedureStaff.Select(s => s.IdUser).ToListAsync();

            var users = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                .Where(u => u.IsActive && !staffUserIds.Contains(u.UserId))
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 3))
                .Where(u => u.Username.Contains(term) || u.Person.FirstName.Contains(term))
                .Select(u => new {
                    id = u.UserId,
                    text = $"{u.Person.FirstName} {u.Person.LastNamePaternal} (@{u.Username})"
                })
                .Take(10)
                .ToListAsync();

            return Json(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Areas = await _context.ProcedureAreas
                .OrderBy(a => a.Name)
                .ToListAsync();

            return PartialView("_CreateModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(int IdUser, int IdArea, string JobPosition, bool IsSuperAdmin)
        {
            try
            {
                bool exists = await _context.ProcedureStaff.AnyAsync(s => s.IdUser == IdUser);
                if (exists)
                {
                    return Json(new { success = false, message = "Este usuario ya tiene un área asignada." });
                }

                var hasAdminRole = await _context.UserRoles.AnyAsync(ur => ur.UserId == IdUser && ur.RoleId == 3);
                if (!hasAdminRole)
                {
                    _context.UserRoles.Add(new users_userrole
                    {
                        UserId = IdUser,
                        RoleId = 3,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    });
                }

                var newStaff = new procedure_staff
                {
                    IdUser = IdUser,
                    IdArea = IdArea,
                    JobPosition = JobPosition,
                    IsSuperAdmin = IsSuperAdmin,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.ProcedureStaff.Add(newStaff);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Personal administrativo asignado con éxito." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult QuickAddUser()
        {
            return PartialView("_AddStaffModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreateStaff(string FirstName, string LastNamePaternal, string LastNameMaternal, string Curp, string Gender, string Email, string Username)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newPerson = new users_person
                {
                    FirstName = FirstName,
                    LastNamePaternal = LastNamePaternal,
                    LastNameMaternal = LastNameMaternal,
                    Curp = Curp,
                    Gender = Gender,
                    Email = Email,
                    Phone = "8990000000",
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _context.Persons.Add(newPerson);
                await _context.SaveChangesAsync();

                var newUser = new users_user
                {
                    PersonId = newPerson.PersonId,
                    Username = Username.ToLower(),
                    Email = Email,
                    PasswordHash = "HASH_TEMPORAL_TEST",
                    IsLocked = false,
                    LockReason = "",
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    LastLoginDate = null
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                var userRole = new users_userrole
                {
                    UserId = newUser.UserId,
                    RoleId = 3,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _context.UserRoles.Add(userRole);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "¡Usuario, Persona y Rol creados correctamente!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Error de base de datos: " + innerMessage });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _context.ProcedureStaff
                .Include(s => s.User)
                    .ThenInclude(u => u.Person)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null) return NotFound();

            ViewBag.Areas = await _context.ProcedureAreas
                .OrderBy(a => a.Name)
                .ToListAsync();

            return PartialView("_EditModal", staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStaff(procedure_staff staffData)
        {
            try
            {
                var existingStaff = await _context.ProcedureStaff.FindAsync(staffData.Id);

                if (existingStaff == null)
                {
                    return Json(new { success = false, message = "No se encontró el registro del personal." });
                }

                existingStaff.IdArea = staffData.IdArea;
                existingStaff.JobPosition = staffData.JobPosition;
                existingStaff.IsSuperAdmin = staffData.IsSuperAdmin;

                _context.ProcedureStaff.Update(existingStaff);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Información de staff actualizada con éxito." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int id)
        {
            var staff = await _context.ProcedureStaff
                .Include(s => s.User).ThenInclude(u => u.Person)
                .Include(s => s.ProcedureArea)
                .Where(s => s.Id == id)
                .Select(s => new StaffViewModel
                {
                    Id = s.Id,
                    FullName = $"{s.User.Person.FirstName} {s.User.Person.LastNamePaternal} {s.User.Person.LastNameMaternal}",
                    AreaName = s.ProcedureArea.Name
                }).FirstOrDefaultAsync();

            if (staff == null) return NotFound();

            return PartialView("_ActivateModal", staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateStaff(int id)
        {
            try
            {
                var staff = await _context.ProcedureStaff.FindAsync(id);
                if (staff == null) return Json(new { success = false });

                staff.IsActive = true;

                var staffRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == staff.IdUser && ur.RoleId == 3);
                if (staffRole != null) staffRole.IsActive = true;

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int id)
        {
            var staff = await _context.ProcedureStaff
                .Include(s => s.User).ThenInclude(u => u.Person)
                .Include(s => s.ProcedureArea)
                .Where(s => s.Id == id)
                .Select(s => new StaffViewModel
                {
                    Id = s.Id,
                    FullName = $"{s.User.Person.FirstName} {s.User.Person.LastNamePaternal} {s.User.Person.LastNameMaternal}",
                    AreaName = s.ProcedureArea.Name
                }).FirstOrDefaultAsync();

            if (staff == null) return NotFound();

            return PartialView("_DeactivateModal", staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateStaff(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var staff = await _context.ProcedureStaff.FindAsync(id);
                if (staff == null) return Json(new { success = false, message = "No encontrado." });

                staff.IsActive = false;

                var staffRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == staff.IdUser && ur.RoleId == 3);
                if (staffRole != null) staffRole.IsActive = false;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "El personal ha sido dado de baja del sistema de trámites." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}
