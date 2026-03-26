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

        // ✅ Ver lista — Filtrada por rol
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

        // ✅ Details — Todos los roles autorizados pueden ver
        public async Task<IActionResult> Details(int id)
        {
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

            if (data == null) return NotFound();

            if (User.IsInRole("Head Nurse") && data.Rol != "Nurse") return View("AccesoDenegado");
            if (User.IsInRole("Head of Psychology") && data.Rol != "Psychologist") return View("AccesoDenegado");

            return View(data);
        }

        // 🔒 Crear — Heads y Master
        [Authorize(Roles = "Head Nurse,Head of Psychology,Master")]
        public IActionResult Create() => View(new CreateMedicalStaffVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Head Nurse,Head of Psychology,Master")]
        public async Task<IActionResult> Create(CreateMedicalStaffVM model)
        {
            if (User.IsInRole("Head Nurse") && model.RoleId != 4)
            {
                ModelState.AddModelError("", "Solo puedes registrar Enfermeros.");
                return View(model);
            }
            if (User.IsInRole("Head of Psychology") && model.RoleId != 5)
            {
                ModelState.AddModelError("", "Solo puedes registrar Psicólogos.");
                return View(model);
            }
            if (User.IsInRole("Master") && !new[] { 6, 7, 9, 10 }.Contains(model.RoleId))
            {
                ModelState.AddModelError("", "Como Master solo puedes crear Jefes, Coordinadores o Masters.");
                return View(model);
            }

            if (!ModelState.IsValid) return View(model);

            // Lógica de guardado...
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

        // 🔒 Editar — Solo Master
        [Authorize(Roles = "Head Nurse,Head of Psychology,Master")]
        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _context.MedicalStaff.FindAsync(id);
            if (staff == null) return NotFound();

            var permiso = await _context.MedicalPermissions.FirstOrDefaultAsync(p => p.StaffId == id);

            // CORRECCIÓN AQUÍ: Se agregan las columnas faltantes con alias para satisfacer al StaffListVM
            var persona = await _context.Database.SqlQueryRaw<StaffListVM>(@"
        SELECT 
            s.staff_id AS Id, 
            p.FirstName + ' ' + p.LastNamePaternal AS NombreCompleto, 
            p.Curp AS Curp,
            '' AS Rol,            -- Columna virtual para evitar error
            '' AS Correo,         -- Columna virtual para evitar error
            '' AS Turno,          -- Columna virtual para evitar error
            GETDATE() AS FechaCreacion -- Columna virtual para evitar error
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
        [Authorize(Roles = "Head Nurse,Head of Psychology,Master")]
        public async Task<IActionResult> Edit(EditMedicalStaffVM model)
        {
            var staff = await _context.MedicalStaff.FindAsync(model.Id);
            if (staff == null) return NotFound();

            staff.RoleId = model.RoleId;
            staff.Shift = model.Shift;

            var permiso = await _context.MedicalPermissions.FirstOrDefaultAsync(p => p.StaffId == model.Id);
            if (permiso != null)
            {
                permiso.Ver = model.Ver; permiso.Agregar = model.Agregar;
                permiso.Modificar = model.Modificar; permiso.Borrar = model.Borrar;
            }

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // 🔒 Eliminar — Solo Master
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Delete(int id)
        {
            var staff = await _context.MedicalStaff.FindAsync(id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staff = await _context.MedicalStaff.FindAsync(id);
            if (staff != null) _context.MedicalStaff.Remove(staff);
            await _context.SaveChangesAsync();
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

            return data == null ? Json(null) : Json(new { personId = data.PersonId, nombre = data.FirstName, correo = data.Email });
        }
    }
}