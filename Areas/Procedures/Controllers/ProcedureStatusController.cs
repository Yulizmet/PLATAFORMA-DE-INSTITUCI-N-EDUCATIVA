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
                .OrderBy(s => s.StepOrder)
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

            procedureStatus.DateUpdated = DateTime.Now;

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
            statusDb.BackgroundColor = procedureStatus.BackgroundColor;
            statusDb.TextColor = procedureStatus.TextColor;
            statusDb.StepOrder = procedureStatus.StepOrder;
            statusDb.DateUpdated = DateTime.Now;

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
            var status = await _context.ProcedureStatus.FindAsync(id);
            if (status == null)
                return Json(new { success = false });

            _context.ProcedureStatus.Remove(status);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}