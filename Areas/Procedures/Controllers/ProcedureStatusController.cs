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
    public class ProcedureStatusController : Controller
    {
        private readonly AppDbContext _context;

        public ProcedureStatusController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var statusList = await _context.ProcedureStatus
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(statusList);
        }

        [HttpGet]
        public IActionResult Create() => PartialView("_CreateModal");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(procedure_status procedureStatus)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = GetErrorsFromModelState() });
            }

            bool exists = await _context.ProcedureStatus.AnyAsync(s => s.InternalCode == procedureStatus.InternalCode);
            if (exists)
            {
                return Json(new { success = false, errors = new[] { "El Código Interno ya existe. Debe ser único." } });
            }

            _context.ProcedureStatus.Add(procedureStatus);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var status = await _context.ProcedureStatus.FindAsync(id);
            if (status == null) return NotFound();

            return PartialView("_EditModal", status);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(procedure_status procedureStatus)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = GetErrorsFromModelState() });
            }

            var statusDb = await _context.ProcedureStatus.FindAsync(procedureStatus.Id);
            if (statusDb == null) return Json(new { success = false });

            statusDb.Name = procedureStatus.Name;
            statusDb.InternalCode = procedureStatus.InternalCode;
            statusDb.BackgroundColor = procedureStatus.BackgroundColor;
            statusDb.TextColor = procedureStatus.TextColor;
            statusDb.IsTerminalState = procedureStatus.IsTerminalState;
            statusDb.IsActionRequiredByUser = procedureStatus.IsActionRequiredByUser;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var status = await _context.ProcedureStatus.FindAsync(id);
            if (status == null) return NotFound();

            return PartialView("_DeleteModal", status);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool isInUse = await _context.ProcedureFlow.AnyAsync(f => f.IdStatus == id);
            if (isInUse)
            {
                return Json(new { success = false, message = "No se puede eliminar: Este estado está siendo usado en una ruta de proceso." });
            }

            var status = await _context.ProcedureStatus.FindAsync(id);
            if (status == null) return Json(new { success = false });

            _context.ProcedureStatus.Remove(status);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private List<string> GetErrorsFromModelState()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
        }
    }
}