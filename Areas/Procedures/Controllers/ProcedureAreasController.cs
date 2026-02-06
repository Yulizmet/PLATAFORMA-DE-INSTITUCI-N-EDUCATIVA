using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class ProcedureAreasController : Controller
    {
        private readonly AppDbContext _context;

        public ProcedureAreasController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.ProcedureAreas.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create() => PartialView("_CreateModal");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(procedure_areas procedureArea)
        {
            ModelState.Remove("Datetime");

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            procedureArea.DateUpdated = DateTime.Now;
            _context.ProcedureAreas.Add(procedureArea);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var area = await _context.ProcedureAreas.FindAsync(id);
            if (area == null) return NotFound();

            return PartialView("_EditModal", area);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(procedure_areas procedureArea)
        {
            ModelState.Remove("Datetime");

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            var areaDb = await _context.ProcedureAreas
                .FirstOrDefaultAsync(x => x.Id == procedureArea.Id);

            if (areaDb == null)
                return Json(new { success = false });

            areaDb.Name = procedureArea.Name;
            areaDb.Description = procedureArea.Description;
            areaDb.DateUpdated = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var area = await _context.ProcedureAreas.FindAsync(id);
            if (area == null) return NotFound();

            return PartialView("_DeleteModal", area);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var area = await _context.ProcedureAreas.FindAsync(id);
            if (area == null)
                return Json(new { success = false });

            _context.ProcedureAreas.Remove(area);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}