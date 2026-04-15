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
    public class ProcedureConfigurationController : _ProceduresBaseController
    {
        public ProcedureConfigurationController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            await LoadPermissions("Trámites");
            var data = await _context.ProcedureTypes
                .Include(p => p.ProcedureArea)
                .Include(p => p.Requirements).ThenInclude(r => r.ProcedureTypeDocument)
                .Include(p => p.ProcedureFlow).ThenInclude(f => f.ProcedureStatus)
                .ToListAsync();

            return View(data);
        }

        private async Task PrepareDropdowns()
        {
            ViewData["IdArea"] = new SelectList(await _context.ProcedureAreas.ToListAsync(), "Id", "Name");
            ViewData["IdTypeDocument"] = new SelectList(await _context.ProcedureTypeDocuments.ToListAsync(), "Id", "Name");

            var filteredStatus = await _context.ProcedureStatus
                .Where(s => s.Name != "Rechazado" && s.Name != "Cancelado")
                .OrderBy(s => s.Id)
                .ToListAsync();

            var statusData = filteredStatus.Select(s => new {
                Id = s.Id,
                Name = s.Name,
                Bg = s.BackgroundColor,
                Txt = s.TextColor,
                IsTerminal = s.IsTerminalState
            }).ToList();

            ViewData["StatusData"] = statusData;
            ViewData["IdStatus"] = new SelectList(filteredStatus, "Id", "Name");
        }

        private async Task EnsureDefaultFlow(int procedureTypeId)
        {
            var rejectedStatus = await _context.ProcedureStatus.FirstOrDefaultAsync(s => s.Name == "Rechazado");
            var cancelledStatus = await _context.ProcedureStatus.FirstOrDefaultAsync(s => s.Name == "Cancelado");

            if (rejectedStatus != null)
            {
                bool existsRejected = await _context.ProcedureFlow
                    .AnyAsync(f => f.IdTypeProcedure == procedureTypeId && f.IdStatus == rejectedStatus.Id);

                if (!existsRejected)
                {
                    _context.ProcedureFlow.Add(new procedure_flow
                    {
                        IdTypeProcedure = procedureTypeId,
                        IdStatus = rejectedStatus.Id,
                        StepOrder = 90
                    });
                }
            }

            if (cancelledStatus != null)
            {
                bool existsCancelled = await _context.ProcedureFlow
                    .AnyAsync(f => f.IdTypeProcedure == procedureTypeId && f.IdStatus == cancelledStatus.Id);

                if (!existsCancelled)
                {
                    _context.ProcedureFlow.Add(new procedure_flow
                    {
                        IdTypeProcedure = procedureTypeId,
                        IdStatus = cancelledStatus.Id,
                        StepOrder = 99
                    });
                }
            }

        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadPermissions("Trámites");
            await PrepareDropdowns();
            return PartialView("_CreateModal");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            await LoadPermissions("Trámites");
            if (id == null) return NotFound();

            var procedureType = await _context.ProcedureTypes
                .Include(p => p.Requirements).ThenInclude(r => r.ProcedureTypeDocument)
                .Include(p => p.ProcedureFlow.Where(f =>
                    f.ProcedureStatus.Name != "Cancelado" &&
                    f.ProcedureStatus.Name != "Rechazado"))
                    .ThenInclude(f => f.ProcedureStatus)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (procedureType == null) return NotFound();

            await PrepareDropdowns();
            return PartialView("_EditModal", procedureType);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            await LoadPermissions("Trámites");
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
            await LoadPermissions("Trámites");
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

        public async Task<IActionResult> BulkEdit()
        {
            await LoadPermissions("Trámites");

            ViewBag.StatusData = await _context.ProcedureStatus
                .Select(s => new {
                    id = s.Id,
                    name = s.Name,
                    bg = s.BackgroundColor,
                    txt = s.TextColor,
                    isTerminal = s.IsTerminalState
                }).ToListAsync();

            ViewBag.IdArea = new SelectList(_context.ProcedureAreas, "Id", "Name");
            ViewBag.IdTypeDocument = new SelectList(_context.ProcedureTypeDocuments, "Id", "Name");
            ViewBag.IdStatus = new SelectList(_context.ProcedureStatus, "Id", "Name");

            return PartialView("_BulkEditModal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProcedureMaster(procedure_types procedureType,
            List<procedure_type_requirements> requirementsList,
            List<procedure_flow> flowList)
        {
            await LoadPermissions("Trámites");

            ModelState.Remove("ProcedureArea");
            ModelState.Remove("Requirements");
            ModelState.Remove("ProcedureFlow");
            ModelState.Remove("DateUpdated");
            var navigationKeys = ModelState.Keys.Where(k => k.Contains("ProcedureTypeDocument") || k.Contains("ProcedureStatus") || k.Contains("ProcedureType")).ToList();
            foreach (var key in navigationKeys) ModelState.Remove(key);

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
                        ptTarget.Requirements = new List<procedure_type_requirements>();
                        ptTarget.ProcedureFlow = new List<procedure_flow>();
                        _context.ProcedureTypes.Add(ptTarget);
                        await _context.SaveChangesAsync();

                        if (requirementsList != null) foreach (var r in requirementsList) { r.IdTypeProcedure = ptTarget.Id; _context.ProcedureTypeRequirements.Add(r); }
                        if (flowList != null) foreach (var f in flowList) { f.IdTypeProcedure = ptTarget.Id; _context.ProcedureFlow.Add(f); }
                    }
                    else
                    {
                        ptTarget = await _context.ProcedureTypes
                            .Include(p => p.Requirements)
                            .Include(p => p.ProcedureFlow)
                            .FirstOrDefaultAsync(p => p.Id == procedureType.Id);

                        if (ptTarget == null) return Json(new { success = false, errors = new[] { "Trámite no encontrado." } });

                        ptTarget.Name = procedureType.Name;
                        ptTarget.IdArea = procedureType.IdArea;
                        ptTarget.DateUpdated = DateTime.Now;

                        if (requirementsList != null)
                        {
                            var newReqIds = requirementsList.Select(r => r.IdTypeDocument).ToList();
                            var reqsParaRemover = ptTarget.Requirements.Where(r => !newReqIds.Contains(r.IdTypeDocument)).ToList();
                            _context.ProcedureTypeRequirements.RemoveRange(reqsParaRemover);

                            foreach (var req in requirementsList)
                            {
                                var existingReq = ptTarget.Requirements.FirstOrDefault(r => r.IdTypeDocument == req.IdTypeDocument);
                                if (existingReq != null) existingReq.IsRequired = req.IsRequired;
                                else _context.ProcedureTypeRequirements.Add(new procedure_type_requirements { IdTypeProcedure = ptTarget.Id, IdTypeDocument = req.IdTypeDocument, IsRequired = req.IsRequired });
                            }
                        }

                        if (flowList != null && flowList.Any())
                        {
                            var currentFlow = ptTarget.ProcedureFlow.OrderBy(f => f.StepOrder).Select(f => new { f.IdStatus, f.StepOrder }).ToList();
                            var newFlow = flowList.OrderBy(f => f.StepOrder).Select(f => new { f.IdStatus, f.StepOrder }).ToList();

                            bool flowsAreDifferent = !currentFlow.SequenceEqual(newFlow);

                            if (flowsAreDifferent)
                            {
                                var stepIds = ptTarget.ProcedureFlow.Select(f => f.Id).ToList();
                                bool hasActiveRequests = await _context.ProcedureRequest.AnyAsync(r => stepIds.Contains(r.IdProcedureFlow));

                                if (hasActiveRequests)
                                {
                                    return Json(new { success = false, errors = new[] { "Se actualizaron los datos básicos, pero el flujo de estados NO se puede modificar porque hay trámites de alumnos en proceso." } });
                                }

                                _context.ProcedureFlow.RemoveRange(ptTarget.ProcedureFlow);
                                foreach (var flow in flowList)
                                {
                                    _context.ProcedureFlow.Add(new procedure_flow
                                    {
                                        IdTypeProcedure = ptTarget.Id,
                                        IdStatus = flow.IdStatus,
                                        StepOrder = flow.StepOrder
                                    });
                                }
                            }
                        }

                        var lastStep = flowList?.OrderByDescending(f => f.StepOrder).FirstOrDefault();
                        if (lastStep != null)
                        {
                            var lastStatus = await _context.ProcedureStatus.FirstOrDefaultAsync(s => s.Id == lastStep.IdStatus);
                            if (lastStatus == null || !lastStatus.IsTerminalState)
                            {
                                return Json(new { success = false, errors = new[] { "El último estado debe ser terminal." } });
                            }
                        }
                    }

                    await EnsureDefaultFlow(ptTarget.Id);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, errors = new[] { "Error: " + (ex.InnerException?.Message ?? ex.Message) } });
                }
            }
            return Json(new { success = false, errors = new[] { "Datos inválidos" } });
        }

        [HttpPost]
        public async Task<IActionResult> SaveBulkUpdate([FromBody] BulkUpdateViewModel data)
        {
            await LoadPermissions("Trámites");

            if (data.FlowList != null && data.FlowList.Any(f => f.StepOrder >= 90))
            {
                return Json(new { success = false, message = "No se pueden usar órdenes >= 90." });
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

                    if (data.RequirementsList != null)
                    {
                        _context.ProcedureTypeRequirements.RemoveRange(p.Requirements);
                        foreach (var req in data.RequirementsList)
                        {
                            p.Requirements.Add(new procedure_type_requirements
                            {
                                IdTypeDocument = req.IdTypeDocument,
                                IsRequired = req.IsRequired
                            });
                        }
                    }

                    if (data.FlowList != null)
                    {
                        var stepIds = p.ProcedureFlow.Select(f => f.Id).ToList();
                        bool hasActiveRequests = await _context.ProcedureRequest.AnyAsync(r => stepIds.Contains(r.IdProcedureFlow));

                        if (!hasActiveRequests)
                        {
                            _context.ProcedureFlow.RemoveRange(p.ProcedureFlow);
                            foreach (var flow in data.FlowList)
                            {
                                p.ProcedureFlow.Add(new procedure_flow
                                {
                                    IdStatus = flow.IdStatus,
                                    StepOrder = flow.StepOrder
                                });
                            }
                        }
                    }

                    await EnsureDefaultFlow(p.Id);
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
            await LoadPermissions("Trámites");
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