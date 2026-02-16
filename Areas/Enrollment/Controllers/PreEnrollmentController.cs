using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Enrollment.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;

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

        // 🔥 AHORA Index REDIRIGE a Create
        // GET: Enrollment/PreEnrollment
        public IActionResult Index()
        {
            return RedirectToAction("Create");
        }

        // GET: Enrollment/PreEnrollment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var preenrollment_general = await _context.preenrollment_general
                .Include(p => p.Career)
                .FirstOrDefaultAsync(m => m.IdData == id);

            if (preenrollment_general == null) return NotFound();

            return View(preenrollment_general);
        }

        // GET: Enrollment/PreEnrollment/Create
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

        // POST: Enrollment/PreEnrollment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PreEnrollmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model.DatosGenerales);
                await _context.SaveChangesAsync();

                // 🔥 Después de guardar vuelve al formulario
                // (Si quieres que vaya a otro paso luego lo cambiamos)
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

        // GET: Enrollment/PreEnrollment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var preenrollment_general = await _context.preenrollment_general.FindAsync(id);
            if (preenrollment_general == null) return NotFound();

            var model = new PreEnrollmentViewModel
            {
                DatosGenerales = preenrollment_general,
                DatosEscolares = new preenrollment_schools(),
                Domicilio = new preenrollment_addresses(),
                Tutor = new preenrollment_tutors(),
                Otros = new preenrollment_infos()
            };

            ViewData["IdCareer"] = new SelectList(
                _context.Set<preenrollment_careers>(),
                "IdCareer",
                "IdCareer",
                preenrollment_general.IdCareer
            );

            return View(model);
        }

        // POST: Enrollment/PreEnrollment/Edit/5
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
                    if (!preenrollment_generalExists(model.DatosGenerales.IdData))
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

        // GET: Enrollment/PreEnrollment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var preenrollment_general = await _context.preenrollment_general
                .Include(p => p.Career)
                .FirstOrDefaultAsync(m => m.IdData == id);

            if (preenrollment_general == null) return NotFound();

            return View(preenrollment_general);
        }

        // POST: Enrollment/PreEnrollment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var preenrollment_general = await _context.preenrollment_general.FindAsync(id);
            if (preenrollment_general != null)
            {
                _context.preenrollment_general.Remove(preenrollment_general);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Create");
        }

        private bool preenrollment_generalExists(int id)
        {
            return _context.preenrollment_general.Any(e => e.IdData == id);
        }
    }
}
