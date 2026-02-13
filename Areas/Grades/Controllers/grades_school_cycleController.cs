using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    public class grades_school_cycleController : Controller
    {
        private readonly AppDbContext _context;

        public grades_school_cycleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/grades_school_cycle
        public async Task<IActionResult> Index()
        {
            return View(await _context.grades_SchoolCycles.ToListAsync());
        }

        // GET: Grades/grades_school_cycle/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_school_cycle = await _context.grades_SchoolCycles
                .FirstOrDefaultAsync(m => m.SchoolCycleId == id);
            if (grades_school_cycle == null)
            {
                return NotFound();
            }

            return View(grades_school_cycle);
        }

        // GET: Grades/grades_school_cycle/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Grades/grades_school_cycle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SchoolCycleId,Name,StartDate,EndDate,IsOpen")] grades_school_cycle grades_school_cycle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grades_school_cycle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(grades_school_cycle);
        }

        // GET: Grades/grades_school_cycle/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_school_cycle = await _context.grades_SchoolCycles.FindAsync(id);
            if (grades_school_cycle == null)
            {
                return NotFound();
            }
            return View(grades_school_cycle);
        }

        // POST: Grades/grades_school_cycle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SchoolCycleId,Name,StartDate,EndDate,IsOpen")] grades_school_cycle grades_school_cycle)
        {
            if (id != grades_school_cycle.SchoolCycleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grades_school_cycle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!grades_school_cycleExists(grades_school_cycle.SchoolCycleId))
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
            return View(grades_school_cycle);
        }

        // GET: Grades/grades_school_cycle/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_school_cycle = await _context.grades_SchoolCycles
                .FirstOrDefaultAsync(m => m.SchoolCycleId == id);
            if (grades_school_cycle == null)
            {
                return NotFound();
            }

            return View(grades_school_cycle);
        }

        // POST: Grades/grades_school_cycle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grades_school_cycle = await _context.grades_SchoolCycles.FindAsync(id);
            if (grades_school_cycle != null)
            {
                _context.grades_SchoolCycles.Remove(grades_school_cycle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool grades_school_cycleExists(int id)
        {
            return _context.grades_SchoolCycles.Any(e => e.SchoolCycleId == id);
        }
    }
}
