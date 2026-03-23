using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.ViewModels;

namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    public class StaffController : Controller
    {
        private readonly AppDbContext _context;

        public StaffController(AppDbContext context)
        {
            _context = context;
        }

        // LISTA PERSONAL
        public async Task<IActionResult> Index()
        {
            var lista = await _context.Database
            .SqlQueryRaw<StaffListVM>(@"
            SELECT
            s.staff_id AS Id,
            p.FirstName + ' ' + p.LastNamePaternal + ' ' + p.LastNameMaternal AS NombreCompleto,
            r.Name AS Rol,
            p.Curp AS Curp,
            u.Email AS Correo,
            s.shift AS Turno,
            s.created_at AS FechaCreacion
            FROM medical_staff s
            INNER JOIN users_person p ON s.PersonId = p.PersonId
            INNER JOIN users_user u ON p.PersonId = u.PersonId
            INNER JOIN users_role r ON s.role_id = r.RoleId
            ORDER BY s.created_at DESC
            ").ToListAsync();

            return View(lista);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var data = await _context.Database
            .SqlQueryRaw<StaffListVM>(@"
            SELECT
                s.staff_id AS Id,
                p.FirstName + ' ' + p.LastNamePaternal + ' ' + p.LastNameMaternal AS NombreCompleto,
                r.Name AS Rol,
                p.Curp AS Curp,
                u.Email AS Correo,
                s.shift AS Turno,
                s.created_at AS FechaCreacion
            FROM medical_staff s
                INNER JOIN users_person p ON s.PersonId = p.PersonId
                INNER JOIN users_user u ON p.PersonId = u.PersonId
                INNER JOIN users_role r ON s.role_id = r.RoleId
                WHERE s.staff_id = {0}
            ", id).FirstOrDefaultAsync();

            if (data == null)
                return NotFound();

            return View(data);
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
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
        public async Task<IActionResult> Create(medical_staff model)
        {
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
                ModelState.AddModelError("", "Este trabajador ya está registrado como personal médico");
                return View(model);
            }

            model.CreatedAt = DateTime.Now;

            _context.Add(model);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Personal médico registrado correctamente.";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Database
            .SqlQueryRaw<StaffListVM>(@"
            SELECT
                s.staff_id AS Id,
                p.FirstName + ' ' + p.LastNamePaternal + ' ' + p.LastNameMaternal AS NombreCompleto,
                r.Name AS Rol,
                p.Curp AS Curp,
                u.Email AS Correo,
                s.shift AS Turno,
                s.created_at AS FechaCreacion
            FROM medical_staff s
            INNER JOIN users_person p ON s.PersonId = p.PersonId
            INNER JOIN users_user u ON p.PersonId = u.PersonId
            INNER JOIN users_role r ON s.role_id = r.RoleId
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
            var staff = await _context.MedicalStaff.FindAsync(id);

            if (staff != null)
            {
                _context.MedicalStaff.Remove(staff);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Personal médico eliminado correctamente.";
                TempData["Tipo"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        // GRAFICAS
        public IActionResult Graficas()
        {
            return View();
        }
    }
}