using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.ViewModels;
using System.Text;

#nullable disable

namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Nurse,Head Nurse,Coordinator,Master")]
    public class LogbookController : Controller
    {
        private readonly AppDbContext _context;

        public LogbookController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Ver Bitácora — Enfermeras, Coordinadores y Master
        public async Task<IActionResult> Index(string matricula)
        {
            ViewData["CurrentFilter"] = matricula;
            string searchString = (matricula ?? string.Empty).Trim();

            var query = from b in _context.MedicalLogbooks
                        join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                        join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                        join per in _context.Persons on pre.UserId equals per.PersonId
                        select new LogBookListVM
                        {
                            Id = b.Id,
                            Folio = b.Folio,
                            Matricula = pre.Matricula,
                            NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                            Motivo = b.MotivoConsulta,
                            Estado = b.Estado,
                            Fecha = b.FechaHora
                        };

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(s => s.Matricula.Contains(searchString));

            var lista = await query.OrderByDescending(x => x.Id).ToListAsync();
            return View(lista);
        }

        // ✅ Reporte — Todos los roles del controlador
        public IActionResult Reporte() => View();

        // 🔒 Crear — Solo Enfermería y Master (Coordinator solo consulta)
        [Authorize(Roles = "Nurse,Head Nurse,Master")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Nurse,Head Nurse,Master")]
        public async Task<IActionResult> Create(medical_logbook bitacora)
        {
            if (bitacora.IdAlumno == 0)
            {
                ModelState.AddModelError("", "Debe buscar un alumno válido con la matrícula");
                return View(bitacora);
            }

            var ultimoFol = await _context.MedicalLogbooks.OrderByDescending(x => x.Id).Select(x => x.Folio).FirstOrDefaultAsync();
            int nuevoNumero = 1;
            if (!string.IsNullOrEmpty(ultimoFol)) { int.TryParse(ultimoFol, out nuevoNumero); nuevoNumero++; }

            bitacora.Folio = nuevoNumero.ToString("D6");
            bitacora.FechaHora = DateTime.Now;
            bitacora.CreatedAt = DateTime.Now;

            _context.MedicalLogbooks.Add(bitacora);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Bitácora creada correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Detalles — Todos los roles del controlador
        public async Task<IActionResult> Details(int id)
        {
            var bitacora = await (
                from b in _context.MedicalLogbooks
                join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where b.Id == id
                select new medical_logbook
                {
                    Id = b.Id,
                    Folio = b.Folio,
                    FechaHora = b.FechaHora,
                    Estado = b.Estado,
                    MotivoConsulta = b.MotivoConsulta,
                    SignosVitales = b.SignosVitales,
                    Observaciones = b.Observaciones,
                    Tratamiento = b.Tratamiento,
                    MatriculaTemp = pre.Matricula,
                    NombreCompletoTemp = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal
                }
            ).FirstOrDefaultAsync();

            if (bitacora == null) return NotFound();
            return View(bitacora);
        }

        // 🔒 Editar — Solo Master
        [Authorize(Roles = "Master,Head Nurse")]
        public async Task<IActionResult> Edit(int id)
        {
            var bitacora = await _context.MedicalLogbooks.FindAsync(id);
            if (bitacora == null) return NotFound();
            return View(bitacora);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Master,Head Nurse")]
        public async Task<IActionResult> Edit(int id, medical_logbook bitacora)
        {
            if (id != bitacora.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var bd = await _context.MedicalLogbooks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                bitacora.CreatedAt = bd.CreatedAt; // Mantener fecha original

                _context.Update(bitacora);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Bitácora actualizada correctamente.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }
            return View(bitacora);
        }

        // 🔒 Eliminar — Solo Master
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Delete(int id)
        {
            var bitacora = await (
                from b in _context.MedicalLogbooks
                join a in _context.MedicalStudents on b.IdAlumno equals a.Id
                join pre in _context.PreenrollmentGenerals on a.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where b.Id == id
                select new medical_logbook
                {
                    Id = b.Id,
                    Folio = b.Folio,
                    FechaHora = b.FechaHora,
                    MatriculaTemp = pre.Matricula,
                    NombreCompletoTemp = per.FirstName + " " + per.LastNamePaternal
                }
            ).FirstOrDefaultAsync();

            if (bitacora == null) return NotFound();
            return View(bitacora);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bitacora = await _context.MedicalLogbooks.FindAsync(id);
            if (bitacora != null)
            {
                _context.MedicalLogbooks.Remove(bitacora);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Registro eliminado con éxito.";
                TempData["Tipo"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- Métodos de apoyo y búsqueda ---

        [HttpGet]
        public async Task<IActionResult> BuscarPorMatricula(string matricula)
        {
            if (string.IsNullOrEmpty(matricula)) return Json(null);
            var data = await (
                from alumno in _context.MedicalStudents
                join pre in _context.PreenrollmentGenerals on alumno.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where pre.Matricula == matricula
                select new { alumnoId = alumno.Id, nombre = per.FirstName, paterno = per.LastNamePaternal, materno = per.LastNameMaternal, sangre = pre.BloodType, peso = alumno.Peso, alergias = alumno.Alergias, condiciones = alumno.CondicionesCronicas }
            ).FirstOrDefaultAsync();
            return Json(data);
        }

        // (Los métodos de Reporte PDF/CSV/Rango se mantienen igual que en tu código original)
    }
}