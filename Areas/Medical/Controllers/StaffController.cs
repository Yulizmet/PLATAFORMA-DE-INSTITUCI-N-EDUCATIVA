using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.ViewModels;

namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Head Nurse,Head of Psychology,Coordinator,Master")]
    public class StaffController : Controller
    {
        private readonly AppDbContext _context;

        public StaffController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            string filtroRol = "";
            if (User.IsInRole("Head Nurse"))
                filtroRol = "AND r.Name = 'Nurse'";
            else if (User.IsInRole("Head of Psychology"))
                filtroRol = "AND r.Name = 'Psychologist'";

            var lista = await _context.Database
                .SqlQueryRaw<StaffListVM>($@"
                    SELECT
                        s.staff_id   AS Id,
                        p.FirstName + ' ' + p.LastNamePaternal + ' ' + p.LastNameMaternal AS NombreCompleto,
                        r.Name       AS Rol,
                        s.role_id    AS RoleId,
                        p.Curp       AS Curp,
                        u.Email      AS Correo,
                        s.shift      AS Turno,
                        s.created_at AS FechaCreacion
                    FROM medical_staff s
                    INNER JOIN users_person p ON s.PersonId = p.PersonId
                    INNER JOIN users_user   u ON p.PersonId = u.PersonId
                    INNER JOIN users_role   r ON s.role_id  = r.RoleId
                    WHERE 1=1 {filtroRol}
                    ORDER BY s.created_at DESC
                ").ToListAsync();

            return View(lista);
        }

        public async Task<IActionResult> Details(int id)
        {
            var data = await _context.Database
                .SqlQueryRaw<StaffListVM>(@"
                    SELECT
                        s.staff_id   AS Id,
                        p.FirstName + ' ' + p.LastNamePaternal + ' ' + p.LastNameMaternal AS NombreCompleto,
                        r.Name AS Rol,
                        s.role_id AS RoleId,
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

            if (data == null) return NotFound();

            if (User.IsInRole("Head Nurse") && data.Rol != "Nurse") return View("AccesoDenegado");
            if (User.IsInRole("Head of Psychology") && data.Rol != "Psychologist") return View("AccesoDenegado");

            return View(data);
        }

        [Authorize(Roles = "Head Nurse,Head of Psychology,Coordinator,Master")]
        public IActionResult Create() => View(new CreateMedicalStaffVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Head Nurse,Head of Psychology,Master")]
        public async Task<IActionResult> Create(CreateMedicalStaffVM model)
        {
            if (User.IsInRole("Head Nurse") && model.RoleId != 18)
            {
                ModelState.AddModelError("", "Solo puedes registrar Enfermeros.");
                return View(model);
            }
            if (User.IsInRole("Head of Psychology") && model.RoleId != 19)
            {
                ModelState.AddModelError("", "Solo puedes registrar Psicólogos.");
                return View(model);
            }
            if (User.IsInRole("Coordinator") && !new[] { 18, 19, 20, 21 }.Contains(model.RoleId))
            {
                ModelState.AddModelError("", "Como Coordinador solo puedes crear Enfermeros, Psicólogos y Jefes.");
                return View(model);
            }
            if (User.IsInRole("Master") && !new[] { 18, 19, 20, 21, 22, 6 }.Contains(model.RoleId))
            {
                ModelState.AddModelError("", "Rol no permitido.");
                return View(model);
            }

            if (!ModelState.IsValid) return View(model);

            var staff = new medical_staff
            {
                PersonId = model.PersonId,
                RoleId = model.RoleId,
                Shift = model.Shift,
                CreatedAt = DateTime.Now
            };

            _context.Add(staff);
            await _context.SaveChangesAsync();

            _context.Add(new medical_permissions
            {
                StaffId = staff.Id,
                Ver = model.Ver,
                Agregar = model.Agregar,
                Modificar = model.Modificar,
                Borrar = model.Borrar
            });
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Personal médico registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Head Nurse,Head of Psychology,Coordinator,Master")]
        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _context.MedicalStaff.FindAsync(id);
            if (staff == null) return NotFound();

            var permiso = await _context.MedicalPermissions.FirstOrDefaultAsync(p => p.StaffId == id);

            var persona = await _context.Database.SqlQueryRaw<StaffListVM>(@"
            SELECT 
                s.staff_id AS Id, 
                p.FirstName + ' ' + p.LastNamePaternal AS NombreCompleto, 
                p.Curp AS Curp,
                '' AS Rol,
                '' AS Correo,
                '' AS Turno,
                GETDATE() AS FechaCreacion,
                s.role_id AS RoleId
            FROM medical_staff s 
            INNER JOIN users_person p ON s.PersonId = p.PersonId 
            WHERE s.staff_id = {0}", id).FirstOrDefaultAsync();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Head Nurse,Head of Psychology,Coordinator,Master")]
        public async Task<IActionResult> Edit(EditMedicalStaffVM model)
        {
            var staff = await _context.MedicalStaff.FindAsync(model.Id);
            if (staff == null) return NotFound();

            if (User.IsInRole("Head Nurse") && model.RoleId != 18)
                return View("AccesoDenegado");

            if (User.IsInRole("Head of Psychology") && model.RoleId != 19)
                return View("AccesoDenegado");

            if (User.IsInRole("Coordinator") && !new[] { 18, 19, 20, 21 }.Contains(model.RoleId))
                return View("AccesoDenegado");

            if (User.IsInRole("Master") && !new[] { 18, 19, 20, 21, 22, 6 }.Contains(model.RoleId))
                return View("AccesoDenegado");

            staff.RoleId = model.RoleId;
            staff.Shift = model.Shift;

            var permiso = await _context.MedicalPermissions.FirstOrDefaultAsync(p => p.StaffId == model.Id);
            if (permiso != null)
            {
                permiso.Ver = model.Ver;
                permiso.Agregar = model.Agregar;
                permiso.Modificar = model.Modificar;
                permiso.Borrar = model.Borrar;
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Database
                .SqlQueryRaw<StaffListVM>(@"
            SELECT
                s.staff_id   AS Id,
                p.FirstName + ' ' + p.LastNamePaternal + ' ' + p.LastNameMaternal AS NombreCompleto,
                r.Name       AS Rol,
                s.role_id    AS RoleId,
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

            if (data == null) return NotFound();

            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Head Nurse,Head of Psychology,Coordinator,Master")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staff = await _context.MedicalStaff.FindAsync(id);
            if (staff == null) return NotFound();

            if (User.IsInRole("Head Nurse") && staff.RoleId != 18)
                return View("AccesoDenegado");

            if (User.IsInRole("Head of Psychology") && staff.RoleId != 19)
                return View("AccesoDenegado");

            if (User.IsInRole("Coordinator") && !new[] { 18, 19, 20, 21 }.Contains(staff.RoleId))
                return View("AccesoDenegado");

            var permisos = await _context.MedicalPermissions
                .Where(p => p.StaffId == id)
                .ToListAsync();

            _context.MedicalPermissions.RemoveRange(permisos);
            _context.MedicalStaff.Remove(staff);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Personal eliminado del módulo médico.";
            TempData["Tipo"] = "danger";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Graficas() => View();

        [HttpGet]
        public async Task<IActionResult> BuscarPorCurp(string curp)
        {
            var data = await _context.Database
                .SqlQueryRaw<StaffSearchVM>(@"
                    SELECT p.PersonId, p.FirstName, p.LastNamePaternal, p.LastNameMaternal, p.Curp, u.Email
                    FROM users_person p
                    INNER JOIN users_user u ON p.PersonId = u.PersonId
                    WHERE p.Curp = {0}
                ", curp).FirstOrDefaultAsync();

            return data == null ? Json(null) : Json(new
            {
                personId = data.PersonId,
                nombre = data.FirstName,
                paterno = data.LastNamePaternal,
                materno = data.LastNameMaternal,
                correo = data.Email
            });
        }
    }
}