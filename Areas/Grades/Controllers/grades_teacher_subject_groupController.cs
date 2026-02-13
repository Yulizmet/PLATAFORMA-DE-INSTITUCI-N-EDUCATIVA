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
    public class grades_teacher_subject_groupController : Controller
    {
        private readonly AppDbContext _context;

        public grades_teacher_subject_groupController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/grades_teacher_subject_group
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.grades_TeacherSubjectGroups.Include(g => g.TeacherSubject).Include(g => g.grades_group);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Grades/grades_teacher_subject_group/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_teacher_subject_group = await _context.grades_TeacherSubjectGroups
                .Include(g => g.TeacherSubject)
                .Include(g => g.grades_group)
                .FirstOrDefaultAsync(m => m.TeacherSubjectGroupId == id);
            if (grades_teacher_subject_group == null)
            {
                return NotFound();
            }

            return View(grades_teacher_subject_group);
        }

        // GET: Grades/grades_teacher_subject_group/Create
        public IActionResult Create()
        {
            ViewData["TeacherSubjectId"] = new SelectList(_context.grades_TeacherSubjects, "TeacherSubjectId", "TeacherSubjectId");
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name");
            return View();
        }

        // POST: Grades/grades_teacher_subject_group/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeacherSubjectGroupId,TeacherSubjectId,GroupId")] grades_teacher_subject_group grades_teacher_subject_group)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grades_teacher_subject_group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeacherSubjectId"] = new SelectList(_context.grades_TeacherSubjects, "TeacherSubjectId", "TeacherSubjectId", grades_teacher_subject_group.TeacherSubjectId);
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name", grades_teacher_subject_group.GroupId);
            return View(grades_teacher_subject_group);
        }

        // GET: Grades/grades_teacher_subject_group/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_teacher_subject_group = await _context.grades_TeacherSubjectGroups.FindAsync(id);
            if (grades_teacher_subject_group == null)
            {
                return NotFound();
            }
            ViewData["TeacherSubjectId"] = new SelectList(_context.grades_TeacherSubjects, "TeacherSubjectId", "TeacherSubjectId", grades_teacher_subject_group.TeacherSubjectId);
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name", grades_teacher_subject_group.GroupId);
            return View(grades_teacher_subject_group);
        }

        // POST: Grades/grades_teacher_subject_group/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeacherSubjectGroupId,TeacherSubjectId,GroupId")] grades_teacher_subject_group grades_teacher_subject_group)
        {
            if (id != grades_teacher_subject_group.TeacherSubjectGroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grades_teacher_subject_group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!grades_teacher_subject_groupExists(grades_teacher_subject_group.TeacherSubjectGroupId))
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
            ViewData["TeacherSubjectId"] = new SelectList(_context.grades_TeacherSubjects, "TeacherSubjectId", "TeacherSubjectId", grades_teacher_subject_group.TeacherSubjectId);
            ViewData["GroupId"] = new SelectList(_context.grades_GradeGroups, "GroupId", "Name", grades_teacher_subject_group.GroupId);
            return View(grades_teacher_subject_group);
        }

        // GET: Grades/grades_teacher_subject_group/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_teacher_subject_group = await _context.grades_TeacherSubjectGroups
                .Include(g => g.TeacherSubject)
                .Include(g => g.grades_group)
                .FirstOrDefaultAsync(m => m.TeacherSubjectGroupId == id);
            if (grades_teacher_subject_group == null)
            {
                return NotFound();
            }

            return View(grades_teacher_subject_group);
        }

        // POST: Grades/grades_teacher_subject_group/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grades_teacher_subject_group = await _context.grades_TeacherSubjectGroups.FindAsync(id);
            if (grades_teacher_subject_group != null)
            {
                _context.grades_TeacherSubjectGroups.Remove(grades_teacher_subject_group);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool grades_teacher_subject_groupExists(int id)
        {
            return _context.grades_TeacherSubjectGroups.Any(e => e.TeacherSubjectGroupId == id);
        }
    }
}
