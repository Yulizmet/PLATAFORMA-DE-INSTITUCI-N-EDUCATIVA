using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Create([Bind("IdData,IdCareer,PaternalLastName,MaternalLastName,Gender,BirthDate,Email,Curp")] preenrollment_general preenrollment_general)
        {
            if (ModelState.IsValid)
            {
                _context.Add(preenrollment_general);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCareer"] = new SelectList(_context.Set<preenrollment_careers>(), "IdCareer", "IdCareer", preenrollment_general.IdCareer);
            return View(preenrollment_general);
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
    }
}
