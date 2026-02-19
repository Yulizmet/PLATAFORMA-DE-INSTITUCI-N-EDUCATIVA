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
    public class grades_grade_levelController : Controller
    {
        private readonly AppDbContext _context;

        public grades_grade_levelController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/grades_grade_level
        public async Task<IActionResult> Index()
        {
            return View(await _context.grades_GradeLevels.ToListAsync());
        }

        // GET: Grades/grades_grade_level/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_grade_level = await _context.grades_GradeLevels
                .FirstOrDefaultAsync(m => m.GradeLevelId == id);
            if (grades_grade_level == null)
            {
                return NotFound();
            }

            return View(grades_grade_level);
        }

        // GET: Grades/grades_grade_level/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Grades/grades_grade_level/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GradeLevelId,Name")] grades_grade_level grades_grade_level)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grades_grade_level);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(grades_grade_level);
        }

        // GET: Grades/grades_grade_level/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_grade_level = await _context.grades_GradeLevels.FindAsync(id);
            if (grades_grade_level == null)
            {
                return NotFound();
            }
            return View(grades_grade_level);
        }

        // POST: Grades/grades_grade_level/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GradeLevelId,Name")] grades_grade_level grades_grade_level)
        {
            if (id != grades_grade_level.GradeLevelId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grades_grade_level);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!grades_grade_levelExists(grades_grade_level.GradeLevelId))
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
            return View(grades_grade_level);
        }

        // GET: Grades/grades_grade_level/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_grade_level = await _context.grades_GradeLevels
                .FirstOrDefaultAsync(m => m.GradeLevelId == id);
            if (grades_grade_level == null)
            {
                return NotFound();
            }

            return View(grades_grade_level);
        }

        // POST: Grades/grades_grade_level/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grades_grade_level = await _context.grades_GradeLevels.FindAsync(id);
            if (grades_grade_level != null)
            {
                _context.grades_GradeLevels.Remove(grades_grade_level);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool grades_grade_levelExists(int id)
        {
            return _context.grades_GradeLevels.Any(e => e.GradeLevelId == id);
        }
    }
}
