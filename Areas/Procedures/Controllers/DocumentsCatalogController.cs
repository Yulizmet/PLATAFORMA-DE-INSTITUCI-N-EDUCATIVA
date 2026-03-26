using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class DocumentsCatalogController : _ProceduresBaseController
    {
        public DocumentsCatalogController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            await LoadPermissions("Documentos");
            return View(await _context.ProcedureTypeDocuments.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadPermissions("Documentos");
            return PartialView("_CreateModal"); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(procedure_type_documents document)
        {
            await LoadPermissions("Documentos");
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
            await LoadPermissions("Documentos");
            if (id == null) return NotFound();

            var doc = await _context.ProcedureTypeDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            return PartialView("_EditModal", doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(procedure_type_documents document)
        {
            await LoadPermissions("Documentos");
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
            await LoadPermissions("Documentos");
            var doc = await _context.ProcedureTypeDocuments.FindAsync(id);
            return PartialView("_DeleteModal", doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await LoadPermissions("Documentos");
            var doc = await _context.ProcedureTypeDocuments.FindAsync(id);
            if (doc == null) return Json(new { success = false, message = "No encontrado." });

            bool inUse = await _context.ProcedureTypeRequirements.AnyAsync(r => r.IdTypeDocument == id) ||
                         await _context.ProcedureDocuments.AnyAsync(d => d.Id == id);

            if (inUse)
                return Json(new { success = false, message = "No se puede eliminar: el documento está vinculado a trámites existentes." });

            _context.ProcedureTypeDocuments.Remove(doc);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Documento eliminado." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            await LoadPermissions("Documentos");
            if (ids == null || !ids.Any()) return Json(new { success = false, message = "Sin selección." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var docs = await _context.ProcedureTypeDocuments.Where(d => ids.Contains(d.Id)).ToListAsync();
                int borrados = 0;
                List<string> fallidos = new List<string>();

                foreach (var doc in docs)
                {
                    bool inUse = await _context.ProcedureTypeRequirements.AnyAsync(r => r.IdTypeDocument == doc.Id) ||
                                 await _context.ProcedureDocuments.AnyAsync(d => d.Id == doc.Id);

                    if (!inUse)
                    {
                        _context.ProcedureTypeDocuments.Remove(doc);
                        borrados++;
                    }
                    else
                    {
                        fallidos.Add(doc.Name);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (fallidos.Any())
                    return Json(new { success = true, message = $"Se borraron {borrados} documentos. No se pudieron borrar {fallidos.Count} por estar en uso: {string.Join(", ", fallidos)}" });

                return Json(new { success = true, message = "Documentos eliminados correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}
