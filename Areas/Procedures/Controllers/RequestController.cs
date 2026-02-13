using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.Services;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class RequestController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IStorageService _storageService;

        public RequestController(AppDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.ProcedureRequest
                .Include(r => r.ProcedureStatus)
                .OrderByDescending(r => r.DateUpdated)
                .ToListAsync();

            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            var procedures = await _context.ProcedureTypes.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Procedures = new SelectList(procedures, "Id", "Name");

            var list = await _context.ProcedureRequest
                .Include(r => r.ProcedureStatus)
                .OrderByDescending(r => r.DateUpdated)
                .ToListAsync();

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetRequirements(int procedureId)
        {
            var requirements = await _context.ProcedureTypeRequirements
                .Include(r => r.ProcedureTypeDocument)
                .Where(r => r.IdTypeProcedure == procedureId)
                .Select(r => new {
                    documentName = r.ProcedureTypeDocument.Name,
                    documentId = r.IdTypeDocument,
                    isRequired = r.IsRequired
                }).ToListAsync();

            return Json(requirements);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(procedure_request request, IFormCollection form)
        {
            request.DateUpdated = DateTime.Now;
            request.IdStatus = 1;
            //request.IdUser = "TEMP_USER";

            if (string.IsNullOrEmpty(request.Folio))
            {
                request.Folio = "FOL-" + DateTime.Now.Ticks.ToString().Substring(10);
            }

            ModelState.Remove("IdUser");
            ModelState.Remove("ProcedureStatus");
            ModelState.Remove("ProcedureType");
            ModelState.Remove("User");
            ModelState.Remove("Datetime");
            ModelState.Remove("Folio");

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Add(request);
                    await _context.SaveChangesAsync();

                    foreach (var file in form.Files)
                    {
                        if (file.Length > 0)
                        {
                            string fileUrl = await _storageService.UploadFileAsync(file, "proceduresfiles");

                            string documentName = file.FileName;

                            var document = new procedure_documents
                            {
                                IdProcedure = request.Id,
                                Name = documentName,
                                FilePath = fileUrl,
                                DateUpdated = DateTime.Now
                            };

                            _context.ProcedureDocuments.Add(document);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, folio = request.Folio });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    return Json(new { success = false, errors = new[] { "Error en la transacción: " + message } });
                }
            }

            var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return Json(new { success = false, errors = modelErrors });
        }

        public async Task<IActionResult> Tracking()
        {
            var requests = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureStatus)
                .Include(r => r.ProcedureDocuments)
                .OrderByDescending(r => r.DateUpdated)
                .ToListAsync();

            foreach (var req in requests)
            {
                foreach (var doc in req.ProcedureDocuments)
                {
                    doc.FilePath = _storageService.GetSecureUrl(doc.FilePath, doc.Name);
                }
            }

            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(string url, string originalName)
        {
            using var httpClient = new HttpClient();
            var buffer = await httpClient.GetByteArrayAsync(url);

            return File(buffer, "application/octet-stream", originalName);
        }

        [HttpGet]
        public async Task<IActionResult> TrackingDetails(int id)
        {
            var request = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureStatus)
                .Include(r => r.ProcedureDocuments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null) return NotFound();

            foreach (var doc in request.ProcedureDocuments)
            {
                doc.FilePath = _storageService.GetSecureUrl(doc.FilePath, doc.Name);
            }

            return PartialView("_TrackingDetails", request);
        }

        [HttpPost]
        public async Task<IActionResult> CancelRequest(int id)
        {
            var request = await _context.ProcedureRequest.FindAsync(id);
            if (request == null) return Json(new { success = false });

            var cancelledStatus = await _context.ProcedureStatus
                .FirstOrDefaultAsync(s => s.Name.Contains("Cancelado") || s.StepOrder == 0);

            if (cancelledStatus != null)
            {
                request.IdStatus = cancelledStatus.Id;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "No se encontró un estado de cancelación configurado." });
        }
    }
}