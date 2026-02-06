using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class ProcedureConfigurationController : Controller
    {
        private readonly AppDbContext _context;

        public ProcedureConfigurationController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.ProcedureTypes
            .Include(p => p.ProcedureArea)
            .Include(p => p.Requirements)
                .ThenInclude(r => r.ProcedureTypeDocument)
            .ToListAsync();

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["IdArea"] = new SelectList(await _context.ProcedureAreas.ToListAsync(), "Id", "Name");
            ViewData["IdTypeDocument"] = new SelectList(await _context.ProcedureTypeDocuments.ToListAsync(), "Id", "Name");

            return PartialView("_CreateModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWithRequirements(procedure_types procedureType, List<procedure_type_requirements> requirementsList)
        {
            ModelState.Remove("ProcedureArea");
            ModelState.Remove("Requirements");
            ModelState.Remove("Datetime");

            if (requirementsList != null)
            {
                for (int i = 0; i < requirementsList.Count; i++)
                {
                    ModelState.Remove($"requirementsList[{i}].ProcedureType");
                    ModelState.Remove($"requirementsList[{i}].ProcedureTypeDocument");
                }
            }

            if (ModelState.IsValid)
            {
                procedureType.DateUpdated = DateTime.Now;

                if (requirementsList != null && requirementsList.Any())
                {
                    foreach (var req in requirementsList)
                    {
                        procedureType.Requirements.Add(req);
                    }
                }

                _context.Add(procedureType);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, errors = errors });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var procedureType = await _context.ProcedureTypes
                .Include(p => p.Requirements)
                    .ThenInclude(r => r.ProcedureTypeDocument)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (procedureType == null) return NotFound();

            ViewData["IdArea"] = new SelectList(await _context.ProcedureAreas.ToListAsync(), "Id", "Name", procedureType.IdArea);
            ViewData["IdTypeDocument"] = new SelectList(await _context.ProcedureTypeDocuments.ToListAsync(), "Id", "Name");

            return PartialView("_EditModal", procedureType);
        }

        [HttpPost]
        public async Task<IActionResult> EditWithRequirements(procedure_types procedureType, List<procedure_type_requirements> requirementsList)
        {
            var ptDb = await _context.ProcedureTypes
                .Include(p => p.Requirements)
                .FirstOrDefaultAsync(p => p.Id == procedureType.Id);

            if (ptDb == null) return Json(new { success = false });

            ptDb.Name = procedureType.Name;
            ptDb.IdArea = procedureType.IdArea;
            ptDb.DateUpdated = DateTime.Now;

            _context.ProcedureTypeRequirements.RemoveRange(ptDb.Requirements);

            if (requirementsList != null)
            {
                foreach (var req in requirementsList)
                {
                    ptDb.Requirements.Add(req);
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var procedureType = await _context.ProcedureTypes
                .Include(p => p.ProcedureArea)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (procedureType == null) return NotFound();

            return PartialView("_DeleteModal", procedureType);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var procedureType = await _context.ProcedureTypes.FindAsync(id);
            if (procedureType != null)
            {
                _context.ProcedureTypes.Remove(procedureType);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}