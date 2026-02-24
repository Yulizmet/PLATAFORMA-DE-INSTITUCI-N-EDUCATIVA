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

        private const string STORAGE_CONNECTION = "AzureStorageProcedures";

        public RequestController(AppDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureFlow).ThenInclude(f => f.ProcedureStatus)
                .Include(r => r.Preenrollments)
                .OrderByDescending(r => r.DateUpdated)
                .ToListAsync();

            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            var procedures = await _context.ProcedureTypes
                .Where(p => !p.Name.Contains("Preinscripción") && !p.Name.Contains("Inscripción"))
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.Procedures = new SelectList(procedures, "Id", "Name");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRequirements(int procedureId)
        {
            var data = await _context.ProcedureTypeRequirements
                .Include(r => r.ProcedureTypeDocument)
                .Where(r => r.IdTypeProcedure == procedureId)
                .ToListAsync();

            var results = data.Select(r => new {
                documentName = r.ProcedureTypeDocument.Name,
                documentId = r.IdTypeDocument,
                isRequired = r.IsRequired
            }).ToList();

            return Json(results);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(procedure_request request, IFormCollection form)
        {
            var initialFlow = await _context.ProcedureFlow
                .Where(f => f.IdTypeProcedure == request.IdTypeProcedure)
                .OrderBy(f => f.StepOrder)
                .FirstOrDefaultAsync();

            if (initialFlow == null)
            {
                return Json(new { success = false, errors = new[] { "Flujo de pasos no configurado." } });
            }

            int currentUserId = 1;

            request.DateCreated = DateTime.Now;
            request.DateUpdated = DateTime.Now;
            request.IdProcedureFlow = initialFlow.Id;
            request.IdUser = currentUserId;

            if (string.IsNullOrEmpty(request.Folio))
            {
                request.Folio = DateTime.Now.Ticks.ToString().Substring(10).ToUpper();
            }

            ModelState.Clear();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ProcedureRequest.Add(request);
                await _context.SaveChangesAsync();

                var student = await _context.PreenrollmentGenerals.FirstOrDefaultAsync(p => p.UserId == currentUserId);
                if (student != null)
                {
                    student.ProcedureRequestId = request.Id;
                    _context.Update(student);
                }

                foreach (var file in form.Files)
                {
                    if (file.Length > 0)
                    {
                        string fileUrl = await _storageService.UploadFileAsync(file, "proceduresfiles", STORAGE_CONNECTION);
                        _context.ProcedureDocuments.Add(new procedure_documents
                        {
                            IdProcedure = request.Id,
                            Name = file.FileName,
                            FilePath = fileUrl,
                            DateUpdated = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, folio = request.Folio });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        public async Task<IActionResult> Tracking()
        {
            var requests = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureFlow)
                    .ThenInclude(f => f.ProcedureStatus)
                .Include(r => r.ProcedureDocuments)
                .OrderByDescending(r => r.DateUpdated)
                .ToListAsync();

            foreach (var req in requests)
            {
                foreach (var doc in req.ProcedureDocuments)
                {
                    doc.FilePath = _storageService.GetSecureUrl(doc.FilePath, doc.Name, STORAGE_CONNECTION);
                }
            }

            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> TrackingDetails(int id)
        {
            var request = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureFlow)
                    .ThenInclude(f => f.ProcedureStatus)
                .Include(r => r.ProcedureDocuments)
                .Include(r => r.ProcedureMonitorings)
                    .ThenInclude(m => m.ProcedureFlow)
                        .ThenInclude(f => f.ProcedureStatus)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null) return NotFound();

            var possibleFlows = await _context.ProcedureFlow
                .Include(f => f.ProcedureStatus)
                .Where(f => f.IdTypeProcedure == request.IdTypeProcedure)
                .OrderBy(f => f.StepOrder)
                .Select(f => new {
                    IdFlow = f.Id,
                    StatusName = f.ProcedureStatus.Name,
                    Order = f.StepOrder,
                    IsCurrent = f.Id == request.IdProcedureFlow
                })
                .ToListAsync();

            ViewBag.PossibleFlows = possibleFlows;

            foreach (var doc in request.ProcedureDocuments)
            {
                doc.FilePath = _storageService.GetSecureUrl(doc.FilePath, doc.Name, STORAGE_CONNECTION);
            }

            return PartialView("_TrackingDetails", request);
        }

        public async Task<IActionResult> PaymentRegistration()
        {
            ViewBag.TramitesPago = await _context.ProcedureTypes
                .Where(p => p.Name.Contains("Preinscripción") || p.Name.Contains("Inscripción"))
                .ToListAsync();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentRequirements(int procedureTypeId)
        {
            var data = await _context.ProcedureTypeRequirements
                .Include(r => r.ProcedureTypeDocument)
                .Where(r => r.IdTypeProcedure == procedureTypeId)
                .ToListAsync();

            var results = data.Select(r => new {
                documentName = r.ProcedureTypeDocument.Name,
                isRequired = r.IsRequired,
                isPaymentDoc = r.ProcedureTypeDocument.Name.ToLower().Contains("pago") ||
                               r.ProcedureTypeDocument.Name.ToLower().Contains("boucher") ||
                               r.ProcedureTypeDocument.Name.ToLower().Contains("comprobante") ||
                               r.ProcedureTypeDocument.Name.ToLower().Contains("recibo")
            }).ToList();

            return Json(results);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentValidation(string identifier, int procedureTypeId, IFormCollection form)
        {
            var student = await _context.PreenrollmentGenerals
                .FirstOrDefaultAsync(p => p.Folio == identifier || p.Matricula == identifier);

            if (student == null)
            {
                return Json(new { success = false, errors = new[] { "No se encontró ningún aspirante o alumno con ese Folio/Matrícula." } });
            }

            var flow = await _context.ProcedureFlow
                .Include(f => f.ProcedureStatus)
                .FirstOrDefaultAsync(f => f.IdTypeProcedure == procedureTypeId &&
                                         (f.ProcedureStatus.Name.Contains("Pago") || f.ProcedureStatus.Name.Contains("Validación")));

            if (flow == null)
            {
                return Json(new { success = false, errors = new[] { "Flujo de validación no encontrado para este trámite." } });
            }

            var request = new procedure_request
            {
                IdTypeProcedure = procedureTypeId,
                IdProcedureFlow = flow.Id,
                IdUser = student.UserId ?? 1,
                DateUpdated = DateTime.Now,
                DateCreated = DateTime.Now,
                Subject = "Validación de pago institucional",
                Message = $"Validación de pago institucional para folio/matrícula: {identifier}",
                Folio = DateTime.Now.Ticks.ToString().Substring(10).ToUpper()
            };

            ModelState.Clear();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ProcedureRequest.Add(request);
                await _context.SaveChangesAsync();

                student.ProcedureRequestId = request.Id;
                _context.Update(student);

                foreach (var file in form.Files)
                {
                    if (file.Length > 0)
                    {
                        string fileUrl = await _storageService.UploadFileAsync(file, "proceduresfiles", STORAGE_CONNECTION);
                        _context.ProcedureDocuments.Add(new procedure_documents
                        {
                            IdProcedure = request.Id,
                            Name = $"Voucher_{identifier}",
                            FilePath = fileUrl,
                            DateUpdated = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, folio = request.Folio });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, errors = new[] { "Error: " + ex.Message } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelRequest(int id)
        {
            var request = await _context.ProcedureRequest.FindAsync(id);
            if (request == null) return Json(new { success = false });

            var cancelledStatus = await _context.ProcedureStatus
                .FirstOrDefaultAsync(s => s.Name.Contains("Cancelado"));

            if (cancelledStatus != null)
            {
                request.IdProcedureFlow = cancelledStatus.Id;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "No se encontró un estado de cancelación configurado." });
        }
    }
}