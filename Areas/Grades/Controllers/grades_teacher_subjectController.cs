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
    public class grades_teacher_subjectController : Controller
    {
        private readonly AppDbContext _context;

        public grades_teacher_subjectController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/grades_teacher_subject
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.grades_TeacherSubjects.Include(g => g.Subject);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Grades/grades_teacher_subject/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_teacher_subject = await _context.grades_TeacherSubjects
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(m => m.TeacherSubjectId == id);
            if (grades_teacher_subject == null)
            {
                return NotFound();
            }

            return View(grades_teacher_subject);
        }

        // GET: Grades/grades_teacher_subject/Create
        public IActionResult Create()
        {
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name");
            return View();
        }

        // POST: Grades/grades_teacher_subject/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeacherSubjectId,TeacherId,SubjectId")] grades_teacher_subject grades_teacher_subject)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grades_teacher_subject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name", grades_teacher_subject.SubjectId);
            return View(grades_teacher_subject);
        }

        // GET: Grades/grades_teacher_subject/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_teacher_subject = await _context.grades_TeacherSubjects.FindAsync(id);
            if (grades_teacher_subject == null)
            {
                return NotFound();
            }
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name", grades_teacher_subject.SubjectId);
            return View(grades_teacher_subject);
        }

        // POST: Grades/grades_teacher_subject/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeacherSubjectId,TeacherId,SubjectId")] grades_teacher_subject grades_teacher_subject)
        {
            if (id != grades_teacher_subject.TeacherSubjectId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grades_teacher_subject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!grades_teacher_subjectExists(grades_teacher_subject.TeacherSubjectId))
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
            ViewData["SubjectId"] = new SelectList(_context.grades_Subjects, "SubjectId", "Name", grades_teacher_subject.SubjectId);
            return View(grades_teacher_subject);
        }

        // GET: Grades/grades_teacher_subject/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grades_teacher_subject = await _context.grades_TeacherSubjects
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(m => m.TeacherSubjectId == id);
            if (grades_teacher_subject == null)
            {
                return NotFound();
            }

            return View(grades_teacher_subject);
        }

        // POST: Grades/grades_teacher_subject/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grades_teacher_subject = await _context.grades_TeacherSubjects.FindAsync(id);
            if (grades_teacher_subject != null)
            {
                _context.grades_TeacherSubjects.Remove(grades_teacher_subject);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool grades_teacher_subjectExists(int id)
        {
            return _context.grades_TeacherSubjects.Any(e => e.TeacherSubjectId == id);
        }
    }
}
