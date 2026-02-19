using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using System;
using System.Collections.Generic;
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

        private string GenerarMatricula()
        {
            int year = DateTime.Now.Year;
            string yearShort = year.ToString().Substring(2, 2);

            int randomNumber = RandomNumberGenerator.GetInt32(100000, 999999);

            return yearShort + randomNumber.ToString() + yearShort;
        }

        private string GenerarFolio(int idGeneration)
        {
            var Generation = _context.Generations
                .FirstOrDefault(g => g.IdGeneration == idGeneration);

            int contador = _context.PreenrollmentGenerals
                .Count(p => p.IdGeneration == idGeneration) + 1;

            string folio = $"{Generation.Year}-{contador.ToString("D4")}";

            return folio;
        }


        // GET: Enrollment/PreEnrollment
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PreenrollmentGenerals.Include(p => p.Career);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Enrollment/PreEnrollment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preenrollment_general = await _context.PreenrollmentGenerals
                .Include(p => p.Career)
                .FirstOrDefaultAsync(m => m.IdData == id);
            if (preenrollment_general == null)
            {
                return NotFound();
            }

            return View(preenrollment_general);
        }

        // GET: Enrollment/PreEnrollment/Create
        public IActionResult Create()
        {
            ViewData["IdCareer"] = new SelectList(_context.Set<preenrollment_careers>(), "IdCareer", "IdCareer");
            return View();
        }

        // POST: Enrollment/PreEnrollment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("IdCareer,PaternalLastName,MaternalLastName,Gender,BirthDate,Email,Curp")]
    preenrollment_general preenrollment_general)

        {
            if (ModelState.IsValid)
            {
                preenrollment_general.Matricula = GenerarMatriculaUnica();
                preenrollment_general.Folio = GenerarFolio(preenrollment_general.IdGeneration);
                preenrollment_general.CreateStat = DateTime.Now;

                _context.Add(preenrollment_general);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }


            ViewData["IdCareer"] = new SelectList(_context.Set<preenrollment_careers>(), "IdCareer", "IdCareer", preenrollment_general.IdCareer);
            return View(preenrollment_general);
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


        // GET: Enrollment/PreEnrollment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preenrollment_general = await _context.PreenrollmentGenerals.FindAsync(id);
            if (preenrollment_general == null)
            {
                return NotFound();
            }
            ViewData["IdCareer"] = new SelectList(_context.Set<preenrollment_careers>(), "IdCareer", "IdCareer", preenrollment_general.IdCareer);
            return View(preenrollment_general);
        }

        // POST: Enrollment/PreEnrollment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdData,IdCareer,PaternalLastName,MaternalLastName,Gender,BirthDate,Email,Curp")] preenrollment_general preenrollment_general)
        {
            if (id != preenrollment_general.IdData)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(preenrollment_general);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!preenrollment_generalExists(preenrollment_general.IdData))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCareer"] = new SelectList(_context.Set<preenrollment_careers>(), "IdCareer", "IdCareer", preenrollment_general.IdCareer);
            return View(preenrollment_general);
        }

        // GET: Enrollment/PreEnrollment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preenrollment_general = await _context.PreenrollmentGenerals
                .Include(p => p.Career)
                .FirstOrDefaultAsync(m => m.IdData == id);
            if (preenrollment_general == null)
            {
                return NotFound();
            }

            return View(preenrollment_general);
        }

        // POST: Enrollment/PreEnrollment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var preenrollment_general = await _context.PreenrollmentGenerals.FindAsync(id);
            if (preenrollment_general != null)
            {
                _context.PreenrollmentGenerals.Remove(preenrollment_general);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool preenrollment_generalExists(int id)
        {
            return _context.PreenrollmentGenerals.Any(e => e.IdData == id);
        }

        // GET: Enrollment/PreEnrollment/Complete
        public IActionResult Complete()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Complete(string folio)
        {
            var pre = await _context.PreenrollmentGenerals
                .FirstOrDefaultAsync(p => p.Folio == folio);

            if (pre == null)
            {
                ModelState.AddModelError("", "Folio no encontrado");
                return View();
            }

            return RedirectToAction("CreateAccount", new { id = pre.IdData });
        }

        public async Task<IActionResult> CreateAccount(int id)
        {
            var pre = await _context.PreenrollmentGenerals.FindAsync(id);

            if (pre == null)
                return NotFound();

            var person = new users_person
            {
            };

            _context.Add(person);
            await _context.SaveChangesAsync();

            var user = new users_user
            {
                PersonId = person.PersonId,
                Username = pre.Matricula,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.Add(user);
            await _context.SaveChangesAsync();

            pre.UserId = user.UserId;
            await _context.SaveChangesAsync();

            return RedirectToAction("Success");
        }


    }
}
