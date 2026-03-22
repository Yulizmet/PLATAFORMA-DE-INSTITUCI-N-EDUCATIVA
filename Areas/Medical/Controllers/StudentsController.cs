using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
#nullable disable


namespace SchoolManager.Areas.Medical.Controllers
{
    [Area("Medical")]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // LISTA GENERAL DE ALUMNOS
        public async Task<IActionResult> Index()
        {
            var alumnos = await (
                from m in _context.MedicalStudents
                join pre in _context.MedicalPreenrollmentGenerals on m.PreenrollmentId equals pre.IdData
                join per in _context.MedicalPersons on pre.UserId equals per.Id
                select new StudentListVM
                {
                    Id = m.Id,
                    Matricula = pre.Matricula,
                    NombreCompleto = per.Nombre + " " + per.ApellidoPaterno + " " + per.ApellidoMaterno,
                    FechaCreacion = m.FechaCreacion
                }
            ).ToListAsync();

            return View(alumnos);
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
        }

        // BUSCAR POR MATRICULA (AJAX)
        [HttpGet]
        public async Task<IActionResult> BuscarPorMatricula(string matricula)
        {
            var data = await (
                from pre in _context.MedicalPreenrollmentGenerals
                join per in _context.MedicalPersons on pre.UserId equals per.Id
                where pre.Matricula == matricula
                select new
                {
                    nombre = per.Nombre,
                    paterno = per.ApellidoPaterno,
                    materno = per.ApellidoMaterno,
                    sangre = pre.BloodType,
                    preenrollmentId = pre.IdData
                }
            ).FirstOrDefaultAsync();

            if (data == null)
                return Json(null);

            return Json(data);
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(medical_student alumno)
        {
            if (alumno.PreenrollmentId == 0)
            {
                ModelState.AddModelError("", "Debe buscar una matrícula válida");
                return View(alumno);
            }

            bool existe = await _context.MedicalStudents
                .AnyAsync(a => a.PreenrollmentId == alumno.PreenrollmentId);

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

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var alumno = await (
                from m in _context.MedicalStudents
                join pre in _context.MedicalPreenrollmentGenerals on m.PreenrollmentId equals pre.IdData
                join per in _context.MedicalPersons on pre.UserId equals per.Id
                where m.Id == id
                select new StudentDetailVM
                {
                    Id = m.Id,
                    Matricula = pre.Matricula,
                    Nombre = per.Nombre,
                    Paterno = per.ApellidoPaterno,
                    Materno = per.ApellidoMaterno,
                    Sangre = pre.BloodType,
                    Peso = m.Peso,
                    Alergias = m.Alergias,
                    CondicionesCronicas = m.CondicionesCronicas,
                    FechaCreacion = m.FechaCreacion
                }
            ).FirstOrDefaultAsync();

            if (alumno == null)
                return NotFound();

            return View(alumno);
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var alumno = await _context.MedicalStudents.FindAsync(id);
            if (alumno == null)
                return NotFound();

            return View(alumno);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, medical_student alumno)
        {
            if (id != alumno.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(alumno);

            var alumnoBD = await _context.MedicalStudents
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumnoBD == null)
                return NotFound();

            alumno.FechaCreacion = alumnoBD.FechaCreacion;

            try
            {
                _context.Update(alumno);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Registro del alumno actualizado correctamente.";
                TempData["Tipo"] = "warning";
            }
            catch (Exception)
            {
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int id)
        {
            var data = await (
                from s in _context.MedicalStudents
                join pre in _context.MedicalPreenrollmentGenerals on s.PreenrollmentId equals pre.IdData
                join per in _context.MedicalPersons on pre.UserId equals per.Id
                where s.Id == id
                select new StudentDetailVM
                {
                    Id = s.Id,
                    Matricula = pre.Matricula,
                    Nombre = per.Nombre,
                    Paterno = per.ApellidoPaterno,
                    Materno = per.ApellidoMaterno,
                    Sangre = pre.BloodType,
                    Peso = s.Peso,
                    Alergias = s.Alergias,
                    CondicionesCronicas = s.CondicionesCronicas,
                    FechaCreacion = s.FechaCreacion
                }
            ).FirstOrDefaultAsync();

            if (data == null)
                return NotFound();

            return View(data);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumno = await _context.MedicalStudents.FindAsync(id);

            if (alumno != null)
            {
                _context.MedicalStudents.Remove(alumno);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Registro médico del alumno eliminado correctamente.";
                TempData["Tipo"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}