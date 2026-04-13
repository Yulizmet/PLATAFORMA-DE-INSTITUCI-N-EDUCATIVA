using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    [Authorize(Roles = "Teacher,Administrator")]

    public class grades_subject_unitController : Controller
    {
        private readonly AppDbContext _context;

        public grades_subject_unitController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/grades_subject_unit
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.grades_SubjectUnits.Include(g => g.Subject);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Grades/grades_subject_unit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_subject_unit = await _context.grades_SubjectUnits
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(m => m.UnitId == id);
            if (grades_subject_unit == null)
            {
                return NotFound();
            }

            return View(grades_subject_unit);
        }

        // GET: Grades/grades_subject_unit/Create
        public IActionResult Create()
        {
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name");
            return View();
        }

        // POST: Grades/grades_subject_unit/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UnitId,SubjectId,UnitNumber,IsOpen")] grades_subject_unit grades_subject_unit)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grades_subject_unit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name", grades_subject_unit.SubjectId);
            return View(grades_subject_unit);
        }

        // GET: Grades/grades_subject_unit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_subject_unit = await _context.grades_SubjectUnits.FindAsync(id);
            if (grades_subject_unit == null)
            {
                return NotFound();
            }
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name", grades_subject_unit.SubjectId);
            return View(grades_subject_unit);
        }

        // POST: Grades/grades_subject_unit/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UnitId,SubjectId,UnitNumber,IsOpen")] grades_subject_unit grades_subject_unit)
        {
            if (id != grades_subject_unit.UnitId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grades_subject_unit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!grades_subject_unitExists(grades_subject_unit.UnitId))
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
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name", grades_subject_unit.SubjectId);
            return View(grades_subject_unit);
        }

        // GET: Grades/grades_subject_unit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_subject_unit = await _context.grades_SubjectUnits
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(m => m.UnitId == id);
            if (grades_subject_unit == null)
            {
                return NotFound();
            }

            return View(grades_subject_unit);
        }

        // POST: Grades/grades_subject_unit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grades_subject_unit = await _context.grades_SubjectUnits.FindAsync(id);
            if (grades_subject_unit != null)
            {
                _context.grades_SubjectUnits.Remove(grades_subject_unit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool grades_subject_unitExists(int id)
        {
            return _context.grades_SubjectUnits.Any(e => e.UnitId == id);
        }
    }
}
