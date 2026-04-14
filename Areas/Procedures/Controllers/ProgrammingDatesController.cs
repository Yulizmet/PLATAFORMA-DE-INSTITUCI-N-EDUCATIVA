using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    [Authorize(Roles = "Administrator")]
    public class ProgrammingDatesController : _ProceduresBaseController
    {
        public ProgrammingDatesController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            await LoadPermissions("Vigencias");
            var procedures = await _context.ProcedureTypes
                .OrderBy(p => p.Name)
                .ToListAsync();
            return View(procedures);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var procedure = await _context.ProcedureTypes.FindAsync(id);
            if (procedure == null) return NotFound();

            return PartialView("_EditModal", procedure);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmed(int id, DateTime? startDate, DateTime? endDate, int? maxResolutionDays)
        {
            var procedure = await _context.ProcedureTypes.FindAsync(id);
            if (procedure == null) return Json(new { success = false, message = "No encontrado" });

            procedure.StartDate = startDate;
            procedure.EndDate = endDate;
            procedure.MaxResolutionDays = maxResolutionDays;
            procedure.DateUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Vigencia actualizada correctamente" });
        }

        [HttpPost]
        public IActionResult GetBulkEditModal(List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest();
            return PartialView("_BulkEditModal", ids);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkEditConfirmed(List<int> ids, DateTime startDate, DateTime endDate, int maxResolutionDays)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No se seleccionaron trámites." });

            try
            {
                var procedures = await _context.ProcedureTypes
                    .Where(p => ids.Contains(p.Id))
                    .ToListAsync();

                foreach (var p in procedures)
                {
                    p.StartDate = startDate;
                    p.EndDate = endDate;
                    p.MaxResolutionDays = maxResolutionDays;
                    p.DateUpdated = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"{procedures.Count} trámites actualizados correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar: " + ex.Message });
            }
        }
    }
}