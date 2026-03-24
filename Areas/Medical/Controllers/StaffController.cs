using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.ViewModels;

namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Nurse,Psychologist,Head Nurse,Head of Psychology,Coordinator,Master")]
    public class StaffController : Controller
    {
        private readonly AppDbContext _context;

        // Nurse y Psychologist no pueden gestionar personal
        private static readonly int[] _rolesRestringidos = { 4, 5 };

        public StaffController(AppDbContext context)
        {
            _context = context;
        }

        // LISTA PERSONAL
        public async Task<IActionResult> Index()
        {
            if (EsRolRestringido())
                return View("AccesoDenegado");

            var lista = await _context.Database
                .SqlQueryRaw<StaffListVM>(@"
                    SELECT
                        s.staff_id   AS Id,
                        p.FirstName + ' ' + p.LastNamePaternal + ' ' + p.LastNameMaternal AS NombreCompleto,
                        r.Name       AS Rol,
                        p.Curp       AS Curp,
                        u.Email      AS Correo,
                        s.shift      AS Turno,
                        s.created_at AS FechaCreacion
                    FROM medical_staff s
                    INNER JOIN users_person p ON s.PersonId = p.PersonId
                    INNER JOIN users_user   u ON p.PersonId = u.PersonId
                    INNER JOIN users_role   r ON s.role_id  = r.RoleId
                    ORDER BY s.created_at DESC
                ").ToListAsync();

            return View(lista);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            if (EsRolRestringido())
                return View("AccesoDenegado");

            var data = await _context.Database
                .SqlQueryRaw<StaffListVM>(@"
                    SELECT
                        s.staff_id   AS Id,
                        p.FirstName + ' ' + p.LastNamePaternal + ' ' + p.LastNameMaternal AS NombreCompleto,
                        r.Name       AS Rol,
                        p.Curp       AS Curp,
                        u.Email      AS Correo,
                        s.shift      AS Turno,
                        s.created_at AS FechaCreacion
                    FROM medical_staff s
                    INNER JOIN users_person p ON s.PersonId = p.PersonId
                    INNER JOIN users_user   u ON p.PersonId = u.PersonId
                    INNER JOIN users_role   r ON s.role_id  = r.RoleId
                    WHERE s.staff_id = {0}
                ", id).FirstOrDefaultAsync();

            if (data == null)
                return NotFound();

            return View(data);
        }

        // CREATE GET
        public IActionResult Create()
        {
            if (EsRolRestringido())
                return View("AccesoDenegado");

            return View(new CreateMedicalStaffVM());
        }

        // BUSCAR POR CURP
        [HttpGet]
        public async Task<IActionResult> BuscarPorCurp(string curp)
        {
            var data = await _context.Database
                .SqlQueryRaw<StaffSearchVM>(@"
                    SELECT
                        p.PersonId,
                        p.FirstName,
                        p.LastNamePaternal,
                        p.LastNameMaternal,
                        p.Curp,
                        u.Email
                    FROM users_person p
                    INNER JOIN users_user u ON p.PersonId = u.PersonId
                    WHERE p.Curp = {0}
                ", curp).FirstOrDefaultAsync();

            if (data == null)
                return Json(null);

            return Json(new
            {
                personId = data.PersonId,
                nombre = data.FirstName,
                paterno = data.LastNamePaternal,
                materno = data.LastNameMaternal,
                correo = data.Email
            });
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMedicalStaffVM model)
        {
            if (EsRolRestringido())
                return View("AccesoDenegado");

            if (model.PersonId == 0)
            {
                ModelState.AddModelError("", "Debes buscar un trabajador primero.");
                return View(model);
            }

            if (model.RoleId == 0)
            {
                ModelState.AddModelError("", "Selecciona un rol.");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Shift))
            {
                ModelState.AddModelError("", "Selecciona un turno.");
                return View(model);
            }

            bool existe = await _context.Set<medical_staff>()
                .AnyAsync(x => x.PersonId == model.PersonId);

            if (existe)
            {
                ModelState.AddModelError("", "Este trabajador ya está registrado como personal médico.");
                return View(model);
            }

            // Obtener UserId a partir del PersonId
            var usuario = await _context.Users
                .FirstOrDefaultAsync(u => u.PersonId == model.PersonId);

            if (usuario == null)
            {
                ModelState.AddModelError("", "No se encontró el usuario asociado.");
                return View(model);
            }

            // Guardar staff
            var staff = new medical_staff
            {
                PersonId = model.PersonId,
                RoleId = model.RoleId,
                Shift = model.Shift,
                CreatedAt = DateTime.Now
            };

            _context.Add(staff);
            await _context.SaveChangesAsync();

            // Guardar permisos
            var permiso = new medical_permissions
            {
                StaffId = staff.Id,
                Ver = model.Ver,
                Agregar = model.Agregar,
                Modificar = model.Modificar,
                Borrar = model.Borrar
            };

            _context.Add(permiso);
            await _context.SaveChangesAsync();

            // Asignar rol en users_userrole si no lo tiene ya
            bool yaTieneRol = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == usuario.UserId
                             && ur.RoleId == model.RoleId
                             && ur.IsActive);

            if (!yaTieneRol)
            {
                _context.UserRoles.Add(new users_userrole
                {
                    UserId = usuario.UserId,
                    RoleId = model.RoleId,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                });
                await _context.SaveChangesAsync();
            }

            TempData["Mensaje"] = "Personal médico registrado correctamente.";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            if (EsRolRestringido())
                return View("AccesoDenegado");

            var staff = await _context.MedicalStaff.FindAsync(id);
            if (staff == null)
                return NotFound();

            var permiso = await _context.MedicalPermissions
                .FirstOrDefaultAsync(p => p.StaffId == id);

            var persona = await _context.Database
                .SqlQueryRaw<StaffListVM>(@"
                    SELECT
                        s.staff_id   AS Id,
                        p.FirstName + ' ' + p.LastNamePaternal + ' ' + p.LastNameMaternal AS NombreCompleto,
                        r.Name       AS Rol,
                        p.Curp       AS Curp,
                        u.Email      AS Correo,
                        s.shift      AS Turno,
                        s.created_at AS FechaCreacion
                    FROM medical_staff s
                    INNER JOIN users_person p ON s.PersonId = p.PersonId
                    INNER JOIN users_user   u ON p.PersonId = u.PersonId
                    INNER JOIN users_role   r ON s.role_id  = r.RoleId
                    WHERE s.staff_id = {0}
                ", id).FirstOrDefaultAsync();

            var vm = new EditMedicalStaffVM
            {
                Id = staff.Id,
                NombreCompleto = persona?.NombreCompleto,
                Curp = persona?.Curp,
                RoleId = staff.RoleId,
                Shift = staff.Shift,
                Ver = permiso?.Ver ?? false,
                Agregar = permiso?.Agregar ?? false,
                Modificar = permiso?.Modificar ?? false,
                Borrar = permiso?.Borrar ?? false
            };

            return View(vm);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditMedicalStaffVM model)
        {
            if (EsRolRestringido())
                return View("AccesoDenegado");

            var staff = await _context.MedicalStaff.FindAsync(model.Id);
            if (staff == null)
                return NotFound();

            staff.RoleId = model.RoleId;
            staff.Shift = model.Shift;

            var permiso = await _context.MedicalPermissions
                .FirstOrDefaultAsync(p => p.StaffId == model.Id);

            if (permiso != null)
            {
                permiso.Ver = model.Ver;
                permiso.Agregar = model.Agregar;
                permiso.Modificar = model.Modificar;
                permiso.Borrar = model.Borrar;
            }
            else
            {
                _context.Add(new medical_permissions
                {
                    StaffId = model.Id,
                    Ver = model.Ver,
                    Agregar = model.Agregar,
                    Modificar = model.Modificar,
                    Borrar = model.Borrar
                });
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Personal médico actualizado correctamente.";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int id)
        {
            if (EsRolRestringido())
                return View("AccesoDenegado");

            var data = await _context.Database
                .SqlQueryRaw<StaffListVM>(@"
                    SELECT
                        s.staff_id   AS Id,
                        p.FirstName + ' ' + p.LastNamePaternal + ' ' + p.LastNameMaternal AS NombreCompleto,
                        r.Name       AS Rol,
                        p.Curp       AS Curp,
                        u.Email      AS Correo,
                        s.shift      AS Turno,
                        s.created_at AS FechaCreacion
                    FROM medical_staff s
                    INNER JOIN users_person p ON s.PersonId = p.PersonId
                    INNER JOIN users_user   u ON p.PersonId = u.PersonId
                    INNER JOIN users_role   r ON s.role_id  = r.RoleId
                    WHERE s.staff_id = {0}
                ", id).FirstOrDefaultAsync();

            if (data == null)
                return NotFound();

            return View(data);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (EsRolRestringido())
                return View("AccesoDenegado");

            var permiso = await _context.MedicalPermissions
                .FirstOrDefaultAsync(p => p.StaffId == id);

            if (permiso != null)
                _context.MedicalPermissions.Remove(permiso);

            var staff = await _context.MedicalStaff.FindAsync(id);
            if (staff != null)
                _context.MedicalStaff.Remove(staff);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Personal médico eliminado correctamente.";
            TempData["Tipo"] = "danger";

            return RedirectToAction(nameof(Index));
        }

        // GRAFICAS
        public IActionResult Graficas()
        {
            if (EsRolRestringido())
                return View("AccesoDenegado");

            return View();
        }

        // HELPER
        private bool EsRolRestringido()
        {
            var staffRoleClaim = User.FindFirst("StaffRoleId");
            if (staffRoleClaim == null) return false;

            if (int.TryParse(staffRoleClaim.Value, out int roleId))
                return _rolesRestringidos.Contains(roleId);

            return false;
        }
    }
}