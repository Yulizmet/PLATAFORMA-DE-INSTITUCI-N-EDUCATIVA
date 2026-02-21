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
    public class grades_gradesController : Controller
    {
        private readonly AppDbContext _context;

        public grades_gradesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/grades_grades
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.grades_Grades.Include(g => g.SubjectUnit);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Grades/grades_grades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_grades = await _context.grades_Grades
                .Include(g => g.SubjectUnit)
                
                .FirstOrDefaultAsync(m => m.GradeId == id);
            if (grades_grades == null)
            {
                return NotFound();
            }

            return View(grades_grades);
        }

        // GET: Grades/grades_grades/Create
        public IActionResult Create()
        {
            ViewData["SubjectUnitId"] = new SelectList(_context.grades_SubjectUnits, "UnitId", "UnitId");
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name");
            return View();
        }

        // POST: Grades/grades_grades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GradeId,StudentId,GroupId,SubjectUnitId,Value,CreatedAt")] grades_grades grades_grades)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grades_grades);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectUnitId"] = new SelectList(_context.grades_SubjectUnits, "UnitId", "UnitId", grades_grades.SubjectUnitId);
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name", grades_grades.GroupId);
            return View(grades_grades);
        }

        // GET: Grades/grades_grades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_grades = await _context.grades_Grades.FindAsync(id);
            if (grades_grades == null)
            {
                return NotFound();
            }
            ViewData["SubjectUnitId"] = new SelectList(_context.grades_SubjectUnits, "UnitId", "UnitId", grades_grades.SubjectUnitId);
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name", grades_grades.GroupId);
            return View(grades_grades);
        }

        // POST: Grades/grades_grades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GradeId,StudentId,GroupId,SubjectUnitId,Value,CreatedAt")] grades_grades grades_grades)
        {
            if (id != grades_grades.GradeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grades_grades);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!grades_gradesExists(grades_grades.GradeId))
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
            ViewData["SubjectUnitId"] = new SelectList(_context.grades_SubjectUnits, "UnitId", "UnitId", grades_grades.SubjectUnitId);
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name", grades_grades.GroupId);
            return View(grades_grades);
        }

        // GET: Grades/grades_grades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_grades = await _context.grades_Grades
                .Include(g => g.SubjectUnit)
              
                .FirstOrDefaultAsync(m => m.GradeId == id);
            if (grades_grades == null)
            {
                return NotFound();
            }

            return View(grades_grades);
        }

        // POST: Grades/grades_grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grades_grades = await _context.grades_Grades.FindAsync(id);
            if (grades_grades != null)
            {
                _context.grades_Grades.Remove(grades_grades);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool grades_gradesExists(int id)
        {
            return _context.grades_Grades.Any(e => e.GradeId == id);
        }
    }
}
