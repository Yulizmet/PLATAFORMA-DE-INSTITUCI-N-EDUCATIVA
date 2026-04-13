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
    [Authorize(Roles = "Teacher")]

    public class grades_unit_recoveryController : Controller
    {
        private readonly AppDbContext _context;

        public grades_unit_recoveryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/grades_unit_recovery
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.grades_UnitRecoveries.Include(g => g.Grade);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Grades/grades_unit_recovery/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_unit_recovery = await _context.grades_UnitRecoveries
                .Include(g => g.Grade)
                .FirstOrDefaultAsync(m => m.UnitRecoveryId == id);
            if (grades_unit_recovery == null)
            {
                return NotFound();
            }

            return View(grades_unit_recovery);
        }

        // GET: Grades/grades_unit_recovery/Create
        public IActionResult Create()
        {
            ViewData["GradeId"] = new SelectList(_context.grades_Grades, "GradeId", "GradeId");
            return View();
        }

        // POST: Grades/grades_unit_recovery/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UnitRecoveryId,GradeId,Value,CreatedAt")] grades_unit_recovery grades_unit_recovery)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grades_unit_recovery);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GradeId"] = new SelectList(_context.grades_Grades, "GradeId", "GradeId", grades_unit_recovery.GradeId);
            return View(grades_unit_recovery);
        }

        // GET: Grades/grades_unit_recovery/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_unit_recovery = await _context.grades_UnitRecoveries.FindAsync(id);
            if (grades_unit_recovery == null)
            {
                return NotFound();
            }
            ViewData["GradeId"] = new SelectList(_context.grades_Grades, "GradeId", "GradeId", grades_unit_recovery.GradeId);
            return View(grades_unit_recovery);
        }

        // POST: Grades/grades_unit_recovery/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UnitRecoveryId,GradeId,Value,CreatedAt")] grades_unit_recovery grades_unit_recovery)
        {
            if (id != grades_unit_recovery.UnitRecoveryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grades_unit_recovery);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!grades_unit_recoveryExists(grades_unit_recovery.UnitRecoveryId))
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
            ViewData["GradeId"] = new SelectList(_context.grades_Grades, "GradeId", "GradeId", grades_unit_recovery.GradeId);
            return View(grades_unit_recovery);
        }

        // GET: Grades/grades_unit_recovery/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_unit_recovery = await _context.grades_UnitRecoveries
                .Include(g => g.Grade)
                .FirstOrDefaultAsync(m => m.UnitRecoveryId == id);
            if (grades_unit_recovery == null)
            {
                return NotFound();
            }

            return View(grades_unit_recovery);
        }

        // POST: Grades/grades_unit_recovery/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grades_unit_recovery = await _context.grades_UnitRecoveries.FindAsync(id);
            if (grades_unit_recovery != null)
            {
                _context.grades_UnitRecoveries.Remove(grades_unit_recovery);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool grades_unit_recoveryExists(int id)
        {
            return _context.grades_UnitRecoveries.Any(e => e.UnitRecoveryId == id);
        }
    }
}
