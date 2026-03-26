using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class AdministrativeStaffController : _ProceduresBaseController
    {
        public AdministrativeStaffController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            await LoadPermissions("Personal");
            var staff = await _context.ProcedureStaff
                .Include(s => s.User).ThenInclude(u => u.Person)
                .Include(s => s.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(s => s.ProcedureArea)
                .Include(s => s.ProcedureJobPosition)
                .Select(s => new StaffViewModel
                {
                    Id = s.Id,
                    UserId = s.IdUser,
                    FullName = $"{s.User.Person.FirstName} {s.User.Person.LastNamePaternal} {s.User.Person.LastNameMaternal}",
                    Username = s.User.Username,
                    Email = s.User.Email,
                    IdJobPosition = s.IdJobPosition,
                    JobPositionName = s.ProcedureJobPosition.Name,
                    AreaName = s.ProcedureArea.Name,
                    IsSuperAdmin = s.IsSuperAdmin,
                    IsActive = s.IsActive,
                    Roles = s.User.UserRoles.Select(ur => ur!.Role!.Name).ToList()
                })
                .ToListAsync();

            return View(staff);
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            await LoadPermissions("Personal");
            var staffUserIds = await _context.ProcedureStaff.Select(s => s.IdUser).ToListAsync();

            var users = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                .Where(u => u.IsActive && !staffUserIds.Contains(u.UserId))
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 3))
                .Where(u => u.Username.Contains(term) || u.Person.FirstName.Contains(term))
                .Select(u => new
                {
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
            await LoadPermissions("Personal");
            ViewBag.Areas = await _context.ProcedureAreas.OrderBy(a => a.Name).ToListAsync();
            ViewBag.JobPositions = await _context.ProcedureJobPosition.OrderBy(j => j.Name).ToListAsync();
            return PartialView("_CreateModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(int IdUser, int IdArea, int IdJobPosition, bool IsSuperAdmin)
        {
            await LoadPermissions("Personal");
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
                    IdJobPosition = IdJobPosition,
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
            LoadPermissions("Personal");
            return PartialView("_AddStaffModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreateStaff(string FirstName, string LastNamePaternal, string LastNameMaternal, string Curp, string Gender, string Email, string Username, string Password)
        {
            await LoadPermissions("Personal");
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
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
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
            await LoadPermissions("Personal");
            var staff = await _context.ProcedureStaff
                .Include(s => s.User).ThenInclude(u => u.Person)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null) return NotFound();

            ViewBag.Areas = await _context.ProcedureAreas.OrderBy(a => a.Name).ToListAsync();

            ViewBag.JobPositions = await _context.ProcedureJobPosition.OrderBy(j => j.Name).ToListAsync();

            return PartialView("_EditModal", staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStaff(procedure_staff staffData)
        {
            await LoadPermissions("Personal");
            try
            {
                var existingStaff = await _context.ProcedureStaff.FindAsync(staffData.Id);

                if (existingStaff == null)
                {
                    return Json(new { success = false, message = "No se encontró el registro del personal." });
                }

                existingStaff.IdArea = staffData.IdArea;
                existingStaff.IdJobPosition = staffData.IdJobPosition;
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
        public async Task<IActionResult> DetailsStaff(int id)
        {
            await LoadPermissions("Personal");
            var staff = await _context.ProcedureStaff
                .Include(s => s.User).ThenInclude(u => u.Person)
                .Include(s => s.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(s => s.ProcedureArea)
                .Include(s => s.ProcedureJobPosition)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null) return NotFound();

            var viewModel = new StaffViewModel
            {
                Id = staff.Id,
                FullName = $"{staff.User.Person.FirstName} {staff.User.Person.LastNamePaternal} {staff.User.Person.LastNameMaternal}",
                Username = staff.User.Username,
                Email = staff.User.Email,
                Curp = staff.User.Person.Curp,
                Gender = staff.User.Person.Gender == "M" ? "Masculino" : "Femenino",
                Nationality = "Mexicana",
                BirthDate = staff.User.Person.BirthDate?.ToString("dd/MM/yyyy") ?? "N/A",
                IdJobPosition = staff.IdJobPosition,
                JobPositionName = staff.ProcedureJobPosition.Name,
                AreaName = staff.ProcedureArea.Name,
                IsSuperAdmin = staff.IsSuperAdmin,
                IsActive = staff.IsActive,
                Roles = staff.User.UserRoles.Select(ur => ur!.Role!.Name).ToList()
            };

            return View("AdministrativeStaff", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditAdministrativeStaff(int id)
        {
            await LoadPermissions("Personal");
            var staff = await _context.ProcedureStaff
                .Include(s => s.User).ThenInclude(u => u.Person)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null || staff.User?.Person == null)
            {
                return NotFound("No se encontraron los datos del personal.");
            }

            var viewModel = new StaffViewModel
            {
                Id = staff.Id,
                PersonId = staff.User.Person.PersonId,
                UserId = staff.IdUser,
                FirstName = staff.User.Person.FirstName,
                LastNamePaternal = staff.User.Person.LastNamePaternal,
                LastNameMaternal = staff.User.Person.LastNameMaternal,
                Curp = staff.User.Person.Curp,
                BirthDate = staff.User.Person.BirthDate?.ToString("yyyy-MM-dd") ?? "",
                Gender = staff.User.Person.Gender,
                IdJobPosition = staff.IdJobPosition,
                IsActive = staff.IsActive,
                IsSuperAdmin = staff.IsSuperAdmin,
                Email = staff.User.Email,
                Username = staff.User.Username
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStaff(StaffViewModel model)
        {
            await LoadPermissions("Personal");
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var staff = await _context.ProcedureStaff
                    .Include(s => s.User).ThenInclude(u => u.Person)
                    .FirstOrDefaultAsync(s => s.Id == model.Id);

                if (staff == null) return Json(new { success = false, message = "Registro no encontrado." });

                staff.User.Person.FirstName = model.FirstName;
                staff.User.Person.LastNamePaternal = model.LastNamePaternal;
                staff.User.Person.LastNameMaternal = model.LastNameMaternal;
                staff.User.Person.Curp = model.Curp;
                staff.User.Person.Gender = model.Gender;
                staff.User.Person.Email = model.Email;

                if (DateTime.TryParse(model.BirthDate, out DateTime fecha))
                    staff.User.Person.BirthDate = fecha;

                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    staff.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                }

                staff.User.Username = model.Username;
                staff.User.Email = model.Email;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Expediente administrativo actualizado correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Error: " + errorMessage });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int id)
        {
            await LoadPermissions("Personal");
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
            await LoadPermissions("Personal");
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
            await LoadPermissions("Personal");
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
            await LoadPermissions("Personal");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            await LoadPermissions("Personal");
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No hay registros seleccionados." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var staffList = await _context.ProcedureStaff
                    .Where(s => ids.Contains(s.Id))
                    .ToListAsync();

                foreach (var staff in staffList)
                {
                    _context.ProcedureStaff.Remove(staff);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = $"{staffList.Count} registro(s) de personal eliminados correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
            }
        }
    }
}
