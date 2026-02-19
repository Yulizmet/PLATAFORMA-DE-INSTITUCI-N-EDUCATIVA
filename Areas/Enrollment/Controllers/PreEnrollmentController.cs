using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Enrollment.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SchoolManager.Areas.Enrollment.Controllers
{
    [Area("Enrollment")]
    public class PreEnrollmentController : Controller
    {
        private readonly AppDbContext _context;

        public PreEnrollmentController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // GENERAR MATRÍCULA
        // =========================
        private string GenerarMatricula()
        {
            int year = DateTime.Now.Year;
            string yearShort = year.ToString().Substring(2, 2);

            int randomNumber = RandomNumberGenerator.GetInt32(100000, 999999);

            return yearShort + randomNumber.ToString() + yearShort;
        }

        private string GenerarMatriculaUnica()
        {
            string matricula;
            do
            {
                matricula = GenerarMatricula();
            }
            while (_context.PreenrollmentGenerals.Any(x => x.Matricula == matricula));

            return matricula;
        }

        private string GenerarFolio(int idGeneration)
        {
            var generation = _context.Generations
                .FirstOrDefault(g => g.IdGeneration == idGeneration);

            int contador = _context.PreenrollmentGenerals
                .Count(p => p.IdGeneration == idGeneration) + 1;

            string folio = $"{generation.Year}-{contador.ToString("D4")}";

            return folio;
        }

        // =========================
        // INDEX (OPCIÓN 1)
        // =========================
        public IActionResult Index()
        {
            return RedirectToAction("Create");
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var preenrollment = await _context.PreenrollmentGenerals
                .Include(p => p.Career)
                .FirstOrDefaultAsync(m => m.IdData == id);

            if (preenrollment == null) return NotFound();

            return View(preenrollment);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            var model = new PreEnrollmentViewModel
            {
                DatosGenerales = new preenrollment_general(),
                DatosEscolares = new preenrollment_schools(),
                Domicilio = new preenrollment_addresses(),
                Tutor = new preenrollment_tutors(),
                Otros = new preenrollment_infos()
            };

            ViewData["IdCareer"] = new SelectList(
                _context.Set<preenrollment_careers>(),
                "IdCareer",
                "IdCareer"
            );

            return View(model);
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PreEnrollmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.DatosGenerales.Matricula = GenerarMatriculaUnica();
                model.DatosGenerales.Folio = GenerarFolio(model.DatosGenerales.IdGeneration);
                model.DatosGenerales.CreateStat = DateTime.Now;

                _context.Add(model.DatosGenerales);
                await _context.SaveChangesAsync();

                return RedirectToAction("Create");
            }

            ViewData["IdCareer"] = new SelectList(
                _context.Set<preenrollment_careers>(),
                "IdCareer",
                "IdCareer",
                model.DatosGenerales.IdCareer
            );

            return View(model);
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var preenrollment = await _context.PreenrollmentGenerals.FindAsync(id);
            if (preenrollment == null) return NotFound();

            var model = new PreEnrollmentViewModel
            {
                DatosGenerales = preenrollment,
                DatosEscolares = new preenrollment_schools(),
                Domicilio = new preenrollment_addresses(),
                Tutor = new preenrollment_tutors(),
                Otros = new preenrollment_infos()
            };

            ViewData["IdCareer"] = new SelectList(
                _context.Set<preenrollment_careers>(),
                "IdCareer",
                "IdCareer",
                preenrollment.IdCareer
            );

            return View(model);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PreEnrollmentViewModel model)
        {
            if (id != model.DatosGenerales.IdData) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model.DatosGenerales);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PreenrollmentGenerals.Any(e => e.IdData == id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction("Create");
            }

            ViewData["IdCareer"] = new SelectList(
                _context.Set<preenrollment_careers>(),
                "IdCareer",
                "IdCareer",
                model.DatosGenerales.IdCareer
            );

            return View(model);
        }

        // =========================
        // DELETE (GET)
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var preenrollment = await _context.PreenrollmentGenerals
                .Include(p => p.Career)
                .FirstOrDefaultAsync(m => m.IdData == id);

            if (preenrollment == null) return NotFound();

            return View(preenrollment);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var preenrollment = await _context.PreenrollmentGenerals.FindAsync(id);

            if (preenrollment != null)
            {
                _context.PreenrollmentGenerals.Remove(preenrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Create");
        }
    }
}
