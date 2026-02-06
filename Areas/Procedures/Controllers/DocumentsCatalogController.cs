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
    public class DocumentsCatalogController : Controller
    {
        private readonly AppDbContext _context;

        public DocumentsCatalogController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.ProcedureTypeDocuments.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create() => PartialView("_CreateModal");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(procedure_type_documents document)
        {
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

            document.DateUpdated = DateTime.Now;
            _context.ProcedureTypeDocuments.Add(document);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var doc = await _context.ProcedureTypeDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            return PartialView("_EditModal", doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(procedure_type_documents document)
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

            var docDb = await _context.ProcedureTypeDocuments
                .FirstOrDefaultAsync(x => x.Id == document.Id);

            if (docDb == null)
                return Json(new { success = false });

            docDb.Name = document.Name;
            docDb.Description = document.Description;
            docDb.DateUpdated = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        public async Task<IActionResult> Delete(int? id)
        {
            var doc = await _context.ProcedureTypeDocuments.FindAsync(id);
            return PartialView("_DeleteModal", doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doc = await _context.ProcedureTypeDocuments.FindAsync(id);
            if (doc == null)
                return Json(new { success = false });

            _context.ProcedureTypeDocuments.Remove(doc);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
