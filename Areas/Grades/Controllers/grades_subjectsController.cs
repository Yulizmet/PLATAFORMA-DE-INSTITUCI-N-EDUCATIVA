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
    public class grades_subjectsController : Controller
    {
        private readonly AppDbContext _context;

        public grades_subjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/grades_subjects
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.grades_Subjects.Include(g => g.GradeLevel);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Grades/grades_subjects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_subjects = await _context.grades_Subjects
                .Include(g => g.GradeLevel)
                .FirstOrDefaultAsync(m => m.SubjectId == id);
            if (grades_subjects == null)
            {
                return NotFound();
            }

            return View(grades_subjects);
        }

        // GET: Grades/grades_subjects/Create
        public IActionResult Create()
        {
            ViewData["GradeLevelId"] = new SelectList(_context.grades_GradeLevels, "GradeLevelId", "Name");
            return View();
        }

        // POST: Grades/grades_subjects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubjectId,Name,GradeLevelId")] grades_subjects grades_subjects)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grades_subjects);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GradeLevelId"] = new SelectList(_context.grades_GradeLevels, "GradeLevelId", "Name", grades_subjects.GradeLevelId);
            return View(grades_subjects);
        }

        // GET: Grades/grades_subjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_subjects = await _context.grades_Subjects.FindAsync(id);
            if (grades_subjects == null)
            {
                return NotFound();
            }
            ViewData["GradeLevelId"] = new SelectList(_context.grades_GradeLevels, "GradeLevelId", "Name", grades_subjects.GradeLevelId);
            return View(grades_subjects);
        }

        // POST: Grades/grades_subjects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubjectId,Name,GradeLevelId")] grades_subjects grades_subjects)
        {
            if (id != grades_subjects.SubjectId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grades_subjects);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!grades_subjectsExists(grades_subjects.SubjectId))
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
            ViewData["GradeLevelId"] = new SelectList(_context.grades_GradeLevels, "GradeLevelId", "Name", grades_subjects.GradeLevelId);
            return View(grades_subjects);
        }

        // GET: Grades/grades_subjects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_subjects = await _context.grades_Subjects
                .Include(g => g.GradeLevel)
                .FirstOrDefaultAsync(m => m.SubjectId == id);
            if (grades_subjects == null)
            {
                return NotFound();
            }

            return View(grades_subjects);
        }

        // POST: Grades/grades_subjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grades_subjects = await _context.grades_Subjects.FindAsync(id);
            if (grades_subjects != null)
            {
                _context.grades_Subjects.Remove(grades_subjects);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool grades_subjectsExists(int id)
        {
            return _context.grades_Subjects.Any(e => e.SubjectId == id);
        }
    }
}
