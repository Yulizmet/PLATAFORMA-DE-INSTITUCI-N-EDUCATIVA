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
    public class grades_groupController : Controller
    {
        private readonly AppDbContext _context;

        public grades_groupController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/grades_group
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.grades_GradeGroups.Include(g => g.GradeLevel).Include(g => g.SchoolCycle);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Grades/grades_group/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_group = await _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .Include(g => g.SchoolCycle)
                .FirstOrDefaultAsync(m => m.GroupId == id);
            if (grades_group == null)
            {
                return NotFound();
            }

            return View(grades_group);
        }

        // GET: Grades/grades_group/Create
        public IActionResult Create()
        {
            ViewData["GradeLevelId"] = new SelectList(_context.grades_GradeLevels, "GradeLevelId", "Name");
            ViewData["SchoolCycleId"] = new SelectList(_context.grades_SchoolCycles, "SchoolCycleId", "Name");
            return View();
        }

        // POST: Grades/grades_group/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupId,Name,GradeLevelId,SchoolCycleId")] grades_group grades_group)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grades_group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GradeLevelId"] = new SelectList(_context.grades_GradeLevels, "GradeLevelId", "Name", grades_group.GradeLevelId);
            ViewData["SchoolCycleId"] = new SelectList(_context.grades_SchoolCycles, "SchoolCycleId", "Name", grades_group.SchoolCycleId);
            return View(grades_group);
        }

        // GET: Grades/grades_group/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_group = await _context.grades_GradeGroups.FindAsync(id);
            if (grades_group == null)
            {
                return NotFound();
            }
            ViewData["GradeLevelId"] = new SelectList(_context.grades_GradeLevels, "GradeLevelId", "Name", grades_group.GradeLevelId);
            ViewData["SchoolCycleId"] = new SelectList(_context.grades_SchoolCycles, "SchoolCycleId", "Name", grades_group.SchoolCycleId);
            return View(grades_group);
        }

        // POST: Grades/grades_group/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupId,Name,GradeLevelId,SchoolCycleId")] grades_group grades_group)
        {
            if (id != grades_group.GroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grades_group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!grades_groupExists(grades_group.GroupId))
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
            ViewData["GradeLevelId"] = new SelectList(_context.grades_GradeLevels, "GradeLevelId", "Name", grades_group.GradeLevelId);
            ViewData["SchoolCycleId"] = new SelectList(_context.grades_SchoolCycles, "SchoolCycleId", "Name", grades_group.SchoolCycleId);
            return View(grades_group);
        }

        // GET: Grades/grades_group/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_group = await _context.grades_GradeGroups
                .Include(g => g.GradeLevel)
                .Include(g => g.SchoolCycle)
                .FirstOrDefaultAsync(m => m.GroupId == id);
            if (grades_group == null)
            {
                return NotFound();
            }

            return View(grades_group);
        }

        // POST: Grades/grades_group/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grades_group = await _context.grades_GradeGroups.FindAsync(id);
            if (grades_group != null)
            {
                _context.grades_GradeGroups.Remove(grades_group);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool grades_groupExists(int id)
        {
            return _context.grades_GradeGroups.Any(e => e.GroupId == id);
        }
    }
}
