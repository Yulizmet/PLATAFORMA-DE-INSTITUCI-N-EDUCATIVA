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
    public class ProcedureAreasController : Controller
    {
        private readonly AppDbContext _context;

        public ProcedureAreasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Grades/ProcedureAreas
        public async Task<IActionResult> Index()
        {
            List<ProcedureAreas> procedureAreas = await _context.ProcedureAreas.ToListAsync();

            return View(procedureAreas);
        }

        public ActionResult GetProcedureAreas() {
            List<ProcedureAreas> List = _context.ProcedureAreas.ToList();
            return Json(new { data = List });
        }

        public ActionResult testVista() {
            return View();
        }

        // GET: Grades/ProcedureAreas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedureAreas = await _context.ProcedureAreas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (procedureAreas == null)
            {
                return NotFound();
            }

            return View(procedureAreas);
        }

        // GET: Grades/ProcedureAreas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Grades/ProcedureAreas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Datetime")] ProcedureAreas procedureAreas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(procedureAreas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(procedureAreas);
        }

        // GET: Grades/ProcedureAreas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedureAreas = await _context.ProcedureAreas.FindAsync(id);
            if (procedureAreas == null)
            {
                return NotFound();
            }
            return View(procedureAreas);
        }

        // POST: Grades/ProcedureAreas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Datetime")] ProcedureAreas procedureAreas)
        {
            if (id != procedureAreas.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(procedureAreas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProcedureAreasExists(procedureAreas.Id))
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
            return View(procedureAreas);
        }

        // GET: Grades/ProcedureAreas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedureAreas = await _context.ProcedureAreas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (procedureAreas == null)
            {
                return NotFound();
            }

            return View(procedureAreas);
        }

        // POST: Grades/ProcedureAreas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var procedureAreas = await _context.ProcedureAreas.FindAsync(id);
            if (procedureAreas != null)
            {
                _context.ProcedureAreas.Remove(procedureAreas);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProcedureAreasExists(int id)
        {
            return _context.ProcedureAreas.Any(e => e.Id == id);
        }
    }
}
