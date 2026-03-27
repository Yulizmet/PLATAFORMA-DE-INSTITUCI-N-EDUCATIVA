using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class ProcedureAreasController : _ProceduresBaseController
    {
        public ProcedureAreasController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            await LoadPermissions("Áreas");
            return View(await _context.ProcedureAreas.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadPermissions("Áreas");
            return PartialView("_CreateModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(procedure_areas procedureArea)
        {
            await LoadPermissions("Áreas");
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
            await LoadPermissions("Áreas");
            if (id == null) return NotFound();

            var area = await _context.ProcedureAreas.FindAsync(id);
            if (area == null) return NotFound();

            return PartialView("_EditModal", area);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(procedure_areas procedureArea)
        {
            await LoadPermissions("Áreas");
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
            await LoadPermissions("Áreas");
            if (id == null) return NotFound();

            var area = await _context.ProcedureAreas.FindAsync(id);
            if (area == null) return NotFound();

            return PartialView("_DeleteModal", area);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await LoadPermissions("Áreas");

            var area = await _context.ProcedureAreas.FindAsync(id);
            if (area == null)
                return Json(new { success = false, message = "El área no existe." });

            try
            {
                bool hasStaff = await _context.ProcedureStaff.AnyAsync(s => s.IdArea == id);
                bool hasRequests = await _context.ProcedureRequest.AnyAsync(r => r.ProcedureType.IdArea == id);
                bool hasTypes = await _context.ProcedureTypes.AnyAsync(t => t.IdArea == id);

                if (hasStaff || hasRequests || hasTypes)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"No se puede eliminar '{area.Name}' porque tiene personal, trámites o configuraciones vinculadas. Intente desactivarla si es necesario."
                    });
                }

                _context.ProcedureAreas.Remove(area);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Área eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al intentar eliminar: " + (ex.InnerException?.Message ?? ex.Message)
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            await LoadPermissions("Áreas");
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No hay registros seleccionados." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var areas = await _context.ProcedureAreas
                    .Where(a => ids.Contains(a.Id))
                    .ToListAsync();

                foreach (var area in areas)
                {
                    bool hasDependencies = await _context.ProcedureRequest.AnyAsync(r => r.ProcedureType.IdArea == area.Id) ||
                                          await _context.ProcedureStaff.AnyAsync(s => s.IdArea == area.Id);

                    if (hasDependencies)
                    {
                        throw new Exception($"El área '{area.Name}' no se puede eliminar porque tiene personal o trámites asociados.");
                    }

                    _context.ProcedureAreas.Remove(area);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = $"{areas.Count} área(s) eliminada(s) correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}