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
    public class grades_final_gradesController : Controller
    {
        private readonly AppDbContext _context;

        public grades_final_gradesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/grades_final_grades
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.grades_FinalGrades.Include(g => g.Subject).Include(g => g.grades_group);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Grades/grades_final_grades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_final_grades = await _context.grades_FinalGrades
                .Include(g => g.Subject)
                .Include(g => g.grades_group)
                .FirstOrDefaultAsync(m => m.FinalGradeId == id);
            if (grades_final_grades == null)
            {
                return NotFound();
            }

            return View(grades_final_grades);
        }

        // GET: Grades/grades_final_grades/Create
        public IActionResult Create()
        {
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name");
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name");
            return View();
        }

        // POST: Grades/grades_final_grades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FinalGradeId,StudentId,SubjectId,GroupId,Value,Passed,CreatedAt")] grades_final_grades grades_final_grades)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grades_final_grades);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name", grades_final_grades.SubjectId);
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name", grades_final_grades.GroupId);
            return View(grades_final_grades);
        }

        // GET: Grades/grades_final_grades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_final_grades = await _context.grades_FinalGrades.FindAsync(id);
            if (grades_final_grades == null)
            {
                return NotFound();
            }
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name", grades_final_grades.SubjectId);
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name", grades_final_grades.GroupId);
            return View(grades_final_grades);
        }

        // POST: Grades/grades_final_grades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FinalGradeId,StudentId,SubjectId,GroupId,Value,Passed,CreatedAt")] grades_final_grades grades_final_grades)
        {
            if (id != grades_final_grades.FinalGradeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grades_final_grades);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!grades_final_gradesExists(grades_final_grades.FinalGradeId))
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
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name", grades_final_grades.SubjectId);
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name", grades_final_grades.GroupId);
            return View(grades_final_grades);
        }

        // GET: Grades/grades_final_grades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_final_grades = await _context.grades_FinalGrades
                .Include(g => g.Subject)
                .Include(g => g.grades_group)
                .FirstOrDefaultAsync(m => m.FinalGradeId == id);
            if (grades_final_grades == null)
            {
                return NotFound();
            }

            return View(grades_final_grades);
        }

        // POST: Grades/grades_final_grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grades_final_grades = await _context.grades_FinalGrades.FindAsync(id);
            if (grades_final_grades != null)
            {
                _context.grades_FinalGrades.Remove(grades_final_grades);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool grades_final_gradesExists(int id)
        {
            return _context.grades_FinalGrades.Any(e => e.FinalGradeId == id);
        }
    }
}
