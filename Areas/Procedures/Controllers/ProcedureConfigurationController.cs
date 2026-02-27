using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.ViewModels;
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
                .Include(p => p.Requirements).ThenInclude(r => r.ProcedureTypeDocument)
                .Include(p => p.ProcedureFlow).ThenInclude(f => f.ProcedureStatus)
                .ToListAsync();

            return View(data);
        }

        //MÉTODOS DE APOYO
        private async Task PrepareDropdowns()
        {
            ViewData["IdArea"] = new SelectList(await _context.ProcedureAreas.ToListAsync(), "Id", "Name");
            ViewData["IdTypeDocument"] = new SelectList(await _context.ProcedureTypeDocuments.ToListAsync(), "Id", "Name");

            var filteredStatus = await _context.ProcedureStatus
                .Where(s => s.Name != "Rechazado" && s.Name != "Cancelado")
                .OrderBy(s => s.Id)
                .ToListAsync();
            ViewData["IdStatus"] = new SelectList(filteredStatus, "Id", "Name");
        }

        private async Task EnsureDefaultFlow(int procedureTypeId)
        {
            var rejectedStatus = await _context.ProcedureStatus.FirstOrDefaultAsync(s => s.Name == "Rechazado");
            var cancelledStatus = await _context.ProcedureStatus.FirstOrDefaultAsync(s => s.Name == "Cancelado");

            if (rejectedStatus != null)
            {
                _context.ProcedureFlow.Add(new procedure_flow { IdTypeProcedure = procedureTypeId, IdStatus = rejectedStatus.Id, StepOrder = 90 });
            }

            if (cancelledStatus != null)
            {
                _context.ProcedureFlow.Add(new procedure_flow { IdTypeProcedure = procedureTypeId, IdStatus = cancelledStatus.Id, StepOrder = 99 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropdowns();
            return PartialView("_CreateModal");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var procedureType = await _context.ProcedureTypes
                .Include(p => p.Requirements).ThenInclude(r => r.ProcedureTypeDocument)
                .Include(p => p.ProcedureFlow).ThenInclude(f => f.ProcedureStatus)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (procedureType == null) return NotFound();
            await PrepareDropdowns();
            return PartialView("_EditModal", procedureType);
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
            var procedureType = await _context.ProcedureTypes
                .Include(p => p.Requirements)
                .Include(p => p.ProcedureFlow)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (procedureType != null)
            {
                _context.ProcedureTypeRequirements.RemoveRange(procedureType.Requirements);
                _context.ProcedureFlow.RemoveRange(procedureType.ProcedureFlow);
                _context.ProcedureTypes.Remove(procedureType);

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> BulkEdit()
        {
            await PrepareDropdowns();
            return PartialView("_BulkEditModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProcedureMaster(procedure_types procedureType,
            List<procedure_type_requirements> requirementsList,
            List<procedure_flow> flowList)
        {
            ModelState.Remove("ProcedureArea");
            ModelState.Remove("Requirements");
            ModelState.Remove("ProcedureFlow");
            ModelState.Remove("DateUpdated");

            if (flowList != null && flowList.Any(f => f.StepOrder >= 90))
            {
                return Json(new { success = false, errors = new[] { "El orden de los pasos debe ser menor a 90. Los números 90 a 99 están reservados." } });
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    procedure_types? ptTarget;

                    if (procedureType.Id == 0)
                    {
                        ptTarget = procedureType;
                        ptTarget.DateUpdated = DateTime.Now;
                        _context.ProcedureTypes.Add(ptTarget);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        ptTarget = await _context.ProcedureTypes
                            .Include(p => p.Requirements)
                            .Include(p => p.ProcedureFlow)
                            .FirstOrDefaultAsync(p => p.Id == procedureType.Id);

                        if (ptTarget == null) return Json(new { success = false, errors = new[] { "No encontrado" } });

                        ptTarget.Name = procedureType.Name;
                        ptTarget.IdArea = procedureType.IdArea;
                        ptTarget.DateUpdated = DateTime.Now;

                        _context.ProcedureTypeRequirements.RemoveRange(ptTarget.Requirements);
                        _context.ProcedureFlow.RemoveRange(ptTarget.ProcedureFlow);
                    }

                    if (requirementsList != null)
                    {
                        foreach (var req in requirementsList) { req.Id = 0; ptTarget.Requirements.Add(req); }
                    }

                    if (flowList != null)
                    {
                        foreach (var flow in flowList) { flow.Id = 0; ptTarget.ProcedureFlow.Add(flow); }
                    }

                    await EnsureDefaultFlow(ptTarget.Id);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, errors = new[] { ex.Message } });
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpPost]
        public async Task<IActionResult> SaveBulkUpdate([FromBody] BulkUpdateViewModel data)
        {
            if (data.FlowList != null && data.FlowList.Any(f => f.StepOrder >= 90))
            {
                return Json(new { success = false, message = "No se pueden usar órdenes >= 90 (reservados para estados automáticos)." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var procedures = await _context.ProcedureTypes
                    .Include(p => p.Requirements)
                    .Include(p => p.ProcedureFlow)
                    .Where(p => data.Ids.Contains(p.Id))
                    .ToListAsync();

                foreach (var p in procedures)
                {
                    if (!string.IsNullOrEmpty(data.Name)) p.Name = data.Name;
                    if (data.IdArea > 0) p.IdArea = data.IdArea;
                    p.DateUpdated = DateTime.Now;

                    if (data.RequirementsList != null && data.RequirementsList.Any())
                    {
                        _context.ProcedureTypeRequirements.RemoveRange(p.Requirements);
                        foreach (var req in data.RequirementsList)
                        {
                            p.Requirements.Add(new procedure_type_requirements { IdTypeDocument = req.IdTypeDocument, IsRequired = req.IsRequired });
                        }
                    }

                    if (data.FlowList != null && data.FlowList.Any())
                    {
                        _context.ProcedureFlow.RemoveRange(p.ProcedureFlow);
                        foreach (var flow in data.FlowList)
                        {
                            p.ProcedureFlow.Add(new procedure_flow { IdStatus = flow.IdStatus, StepOrder = flow.StepOrder });
                        }

                        await EnsureDefaultFlow(p.Id);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return Json(new { success = false, message = "No hay selecciones." });

            var itemsToDelete = await _context.ProcedureTypes
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            _context.ProcedureTypes.RemoveRange(itemsToDelete);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}