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

    public class grades_extraordinary_gradesController : Controller
    {
        private readonly AppDbContext _context;

        public grades_extraordinary_gradesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/grades_extraordinary_grades
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.grades_ExtraordinaryGrades.Include(g => g.FinalGrade);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Grades/grades_extraordinary_grades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_extraordinary_grades = await _context.grades_ExtraordinaryGrades
                .Include(g => g.FinalGrade)
                .FirstOrDefaultAsync(m => m.ExtraordinaryGradeId == id);
            if (grades_extraordinary_grades == null)
            {
                return NotFound();
            }

            return View(grades_extraordinary_grades);
        }

        // GET: Grades/grades_extraordinary_grades/Create
        public IActionResult Create()
        {
            ViewData["FinalGradeId"] = new SelectList(_context.grades_FinalGrades, "FinalGradeId", "FinalGradeId");
            return View();
        }

        // POST: Grades/grades_extraordinary_grades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ExtraordinaryGradeId,FinalGradeId,Value,CreatedAt")] grades_extraordinary_grades grades_extraordinary_grades)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grades_extraordinary_grades);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FinalGradeId"] = new SelectList(_context.grades_FinalGrades, "FinalGradeId", "FinalGradeId", grades_extraordinary_grades.FinalGradeId);
            return View(grades_extraordinary_grades);
        }

        // GET: Grades/grades_extraordinary_grades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_extraordinary_grades = await _context.grades_ExtraordinaryGrades.FindAsync(id);
            if (grades_extraordinary_grades == null)
            {
                return NotFound();
            }
            ViewData["FinalGradeId"] = new SelectList(_context.grades_FinalGrades, "FinalGradeId", "FinalGradeId", grades_extraordinary_grades.FinalGradeId);
            return View(grades_extraordinary_grades);
        }

        // POST: Grades/grades_extraordinary_grades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ExtraordinaryGradeId,FinalGradeId,Value,CreatedAt")] grades_extraordinary_grades grades_extraordinary_grades)
        {
            if (id != grades_extraordinary_grades.ExtraordinaryGradeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grades_extraordinary_grades);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!grades_extraordinary_gradesExists(grades_extraordinary_grades.ExtraordinaryGradeId))
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
            ViewData["FinalGradeId"] = new SelectList(_context.grades_FinalGrades, "FinalGradeId", "FinalGradeId", grades_extraordinary_grades.FinalGradeId);
            return View(grades_extraordinary_grades);
        }

        // GET: Grades/grades_extraordinary_grades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_extraordinary_grades = await _context.grades_ExtraordinaryGrades
                .Include(g => g.FinalGrade)
                .FirstOrDefaultAsync(m => m.ExtraordinaryGradeId == id);
            if (grades_extraordinary_grades == null)
            {
                return NotFound();
            }

            return View(grades_extraordinary_grades);
        }

        // POST: Grades/grades_extraordinary_grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grades_extraordinary_grades = await _context.grades_ExtraordinaryGrades.FindAsync(id);
            if (grades_extraordinary_grades != null)
            {
                _context.grades_ExtraordinaryGrades.Remove(grades_extraordinary_grades);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool grades_extraordinary_gradesExists(int id)
        {
            return _context.grades_ExtraordinaryGrades.Any(e => e.ExtraordinaryGradeId == id);
        }
    }
}
