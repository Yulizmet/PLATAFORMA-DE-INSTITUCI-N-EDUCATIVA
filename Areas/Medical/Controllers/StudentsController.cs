using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.ViewModels; // Asegúrate de tener este namespace para las VM

#nullable disable

namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Nurse,Psychologist,Head Nurse,Head of Psychology,Coordinator,Master")]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Ver lista — Todos los roles autorizados
        public async Task<IActionResult> Index()
        {
            var alumnos = await (
                from m in _context.MedicalStudents
                join pre in _context.PreenrollmentGenerals on m.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                select new StudentListVM
                {
                    Id = m.Id,
                    Matricula = pre.Matricula,
                    NombreCompleto = per.FirstName + " " + per.LastNamePaternal + " " + per.LastNameMaternal,
                    FechaCreacion = m.FechaCreacion
                }
            ).ToListAsync();

            return View(alumnos);
        }

        // ✅ Details — Todos los roles autorizados
        public async Task<IActionResult> Details(int id)
        {
            var alumno = await (
                from m in _context.MedicalStudents
                join pre in _context.PreenrollmentGenerals on m.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where m.Id == id
                select new StudentDetailVM
                {
                    Id = m.Id,
                    Matricula = pre.Matricula,
                    Nombre = per.FirstName,
                    Paterno = per.LastNamePaternal,
                    Materno = per.LastNameMaternal,
                    Sangre = pre.BloodType,
                    Peso = m.Peso,
                    Alergias = m.Alergias,
                    CondicionesCronicas = m.CondicionesCronicas,
                    FechaCreacion = m.FechaCreacion
                }
            ).FirstOrDefaultAsync();

            if (alumno == null) return NotFound();
            return View(alumno);
        }

        // 🔒 Crear — Personal operativo y jefes (No Coordinator)
        [Authorize(Roles = "Nurse,Psychologist,Head Nurse,Head of Psychology,Master")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Nurse,Psychologist,Head Nurse,Head of Psychology,Master")]
        public async Task<IActionResult> Create(medical_student alumno)
        {
            if (alumno.PreenrollmentId == 0)
            {
                ModelState.AddModelError("", "Debe buscar una matrícula válida");
                return View(alumno);
            }

            bool existe = await _context.MedicalStudents.AnyAsync(a => a.PreenrollmentId == alumno.PreenrollmentId);
            if (existe)
            {
                ModelState.AddModelError("", "Este alumno ya tiene registro médico");
                return View(alumno);
            }

            alumno.FechaCreacion = DateTime.Now;
            _context.MedicalStudents.Add(alumno);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Registro médico del alumno creado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // 🔒 Editar — Solo Master
        [Authorize(Roles = "Head Nurse,Head of Psychology,Master")]
        public async Task<IActionResult> Edit(int id)
        {
            var alumno = await _context.MedicalStudents.FindAsync(id);
            if (alumno == null) return NotFound();
            return View(alumno);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Head Nurse,Head of Psychology,Master")]
        public async Task<IActionResult> Edit(int id, medical_student alumno)
        {
            if (id != alumno.Id) return NotFound();
            if (!ModelState.IsValid) return View(alumno);

            var alumnoBD = await _context.MedicalStudents.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (alumnoBD == null) return NotFound();

            alumno.FechaCreacion = alumnoBD.FechaCreacion;
            _context.Update(alumno);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Registro actualizado correctamente.";
            TempData["Tipo"] = "warning";
            return RedirectToAction(nameof(Index));
        }

        // 🔒 Eliminar — Solo Master
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await (
                from s in _context.MedicalStudents
                join pre in _context.PreenrollmentGenerals on s.PreenrollmentId equals pre.IdData
                join per in _context.Persons on pre.UserId equals per.PersonId
                where s.Id == id
                select new StudentDetailVM
                {
                    Id = s.Id,
                    Matricula = pre.Matricula,
                    Nombre = per.FirstName,
                    Paterno = per.LastNamePaternal,
                    Materno = per.LastNameMaternal,
                    Sangre = pre.BloodType,
                    FechaCreacion = s.FechaCreacion
                }
            ).FirstOrDefaultAsync();

            if (data == null) return NotFound();
            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumno = await _context.MedicalStudents.FindAsync(id);
            if (alumno != null)
            {
                _context.MedicalStudents.Remove(alumno);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Registro eliminado correctamente.";
                TempData["Tipo"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }

        // ✅ Auxiliar para búsqueda (AJAX)
        [HttpGet]
        public async Task<IActionResult> BuscarPorMatricula(string matricula)
        {
            var data = await (
                from pre in _context.PreenrollmentGenerals
                join per in _context.Persons on pre.UserId equals per.PersonId
                where pre.Matricula == matricula
                select new
                {
                    nombre = per.FirstName,
                    paterno = per.LastNamePaternal,
                    materno = per.LastNameMaternal,
                    sangre = pre.BloodType,
                    preenrollmentId = pre.IdData
                }
            ).FirstOrDefaultAsync();

            return data == null ? Json(null) : Json(data);
        }
    }
}