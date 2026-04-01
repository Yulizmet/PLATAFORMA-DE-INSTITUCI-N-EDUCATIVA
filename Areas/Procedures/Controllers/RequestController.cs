using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.Services;
using System.Security.Claims;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    
    public class RequestController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IStorageService _storageService;
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ??
              User.FindFirst("UserId")?.Value ?? "0");

        private const string STORAGE_CONNECTION = "AzureStorageProcedures";

        public RequestController(AppDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Administrator"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var now = DateTime.Now;
            bool isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            string targetTerm = isAuthenticated ? "Inscripción" : "Preinscripción";

            List<procedure_request> requests = new List<procedure_request>();
            if (isAuthenticated)
            {
                requests = await _context.ProcedureRequest
                    .Include(r => r.ProcedureType)
                    .Include(r => r.ProcedureFlow).ThenInclude(f => f.ProcedureStatus)
                    .Include(r => r.Preenrollments)
                    .Where(r => r.IdUser == CurrentUserId)
                    .ToListAsync();
            }

            ViewBag.HasActivePayments = await _context.ProcedureTypes
                .AnyAsync(p => p.Name.Contains(targetTerm) &&
                               (!p.StartDate.HasValue || now >= p.StartDate) &&
                               (!p.EndDate.HasValue || now <= p.EndDate));

            if (!(bool)ViewBag.HasActivePayments)
            {
                ViewBag.NextPaymentOpening = await _context.ProcedureTypes
                    .Where(p => p.Name.Contains(targetTerm) && p.StartDate.HasValue && p.StartDate > now)
                    .OrderBy(p => p.StartDate)
                    .Select(p => p.StartDate)
                    .FirstOrDefaultAsync();
            }

            ViewBag.HasActiveGeneralProcedures = await _context.ProcedureTypes
                .AnyAsync(p => !p.Name.Contains("Inscripción") &&
                               !p.Name.Contains("Preinscripción") &&
                               (!p.StartDate.HasValue || now >= p.StartDate) &&
                               (!p.EndDate.HasValue || now <= p.EndDate));

            return View(requests);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> PublicSearch(string folio, string identifier)
        {
            if (string.IsNullOrEmpty(folio) || string.IsNullOrEmpty(identifier))
            {
                return Json(new { success = false, message = "Por favor, ingresa ambos datos." });
            }

            var request = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureFlow).ThenInclude(f => f.ProcedureStatus)
                .Include(r => r.ProcedureMonitorings).ThenInclude(m => m.ProcedureFlow).ThenInclude(f => f.ProcedureStatus)
                .Where(r => r.Folio == folio)
                .FirstOrDefaultAsync();

            var student = await _context.PreenrollmentGenerals
                .AnyAsync(p => p.UserId == request!.IdUser && (p.Matricula == identifier || p.Folio == identifier));

            if (request == null || !student)
            {
                return Json(new { success = false, message = "No se encontró ningún trámite con esa combinación de datos." });
            }

            var viewModel = new PublicTrackingViewModel
            {
                Folio = request.Folio,
                ProcedureName = request.ProcedureType?.Name!,
                StatusName = request.ProcedureFlow?.ProcedureStatus?.Name!,
                BackgroundColor = request.ProcedureFlow?.ProcedureStatus?.BackgroundColor!,
                TextColor = request.ProcedureFlow?.ProcedureStatus?.TextColor!,
                LastUpdate = request.DateUpdated ?? request.DateCreated,
                AdminComment = request.ProcedureMonitorings?.OrderByDescending(m => m.DateUpdated!).Select(m => m.Comment!).FirstOrDefault()
            };

            return PartialView("_PublicTrackingResult", viewModel);
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            var now = DateTime.Now;

            var allProcedures = await _context.ProcedureTypes
                .Where(p => !p.Name.Contains("Preinscripción") && !p.Name.Contains("Inscripción"))
                .OrderBy(p => p.Name)
                .ToListAsync();

            var availableProcedures = allProcedures
                .Where(p => (!p.StartDate.HasValue || now >= p.StartDate) &&
                            (!p.EndDate.HasValue || now <= p.EndDate))
                .ToList();

            ViewBag.UpcomingProcedures = allProcedures
                .Where(p => p.StartDate.HasValue && now < p.StartDate)
                .ToList();

            ViewBag.Procedures = new SelectList(availableProcedures, "Id", "Name");

            var studentData = await _context.PreenrollmentGenerals
                .Where(p => p.UserId == CurrentUserId)
                .Select(p => p.Matricula)
                .FirstOrDefaultAsync();

            ViewBag.UserMatricula = studentData ?? "";

            return View();
        }

        [Authorize(Roles = "Student")]
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

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(procedure_request request, IFormCollection form)
        {
            var availability = await CheckProcedureAvailability(request.IdTypeProcedure);
            if (!availability.IsAvailable)
            {
                return Json(new { success = false, errors = new[] { availability.Message } });
            }

            var initialFlow = await _context.ProcedureFlow
                .Where(f => f.IdTypeProcedure == request.IdTypeProcedure)
                .OrderBy(f => f.StepOrder)
                .FirstOrDefaultAsync();

            if (initialFlow == null)
            {
                return Json(new { success = false, errors = new[] { "Flujo de pasos no configurado." } });
            }

            if (CurrentUserId == 0) return Unauthorized();

            request.DateCreated = DateTime.Now;
            request.IdProcedureFlow = initialFlow.Id;
            request.IdUser = CurrentUserId;

            string yearPrefix = DateTime.Now.ToString("yy");

            var lastRequest = await _context.ProcedureRequest
                .Where(r => r.Folio.StartsWith(yearPrefix))
                .OrderByDescending(r => r.Folio)
                .FirstOrDefaultAsync();

            string newFolio;

            if (lastRequest == null)
            {
                newFolio = yearPrefix + "000001";
            }
            else
            {
                if (int.TryParse(lastRequest.Folio.Substring(2), out int lastNumber))
                {
                    newFolio = yearPrefix + (lastNumber + 1).ToString("D6");
                }
                else
                {
                    newFolio = yearPrefix + "000001";
                }
            }

            if (string.IsNullOrEmpty(request.Folio))
            {
                request.Folio = newFolio;
            }

            ModelState.Clear();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ProcedureRequest.Add(request);
                await _context.SaveChangesAsync();

                var student = await _context.PreenrollmentGenerals.FirstOrDefaultAsync(p => p.UserId == CurrentUserId);
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

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Tracking()
        {
            var requests = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureFlow)
                    .ThenInclude(f => f.ProcedureStatus)
                .Include(r => r.ProcedureDocuments)
                .Where(r => r.IdUser == CurrentUserId)
                .OrderByDescending(r => r.DateCreated)
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

        [Authorize(Roles = "Student")]
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

            if (request.IdUser != CurrentUserId)
            {
                return Unauthorized();
            }

            foreach (var doc in request.ProcedureDocuments)
            {
                doc.FilePath = _storageService.GetSecureUrl(doc.FilePath, doc.Name, STORAGE_CONNECTION);
            }

            var viewModel = new ExtraInfoViewModel
            {
                RequestId = request.Id,
                Folio = request.Folio ?? "S/F",
                ProcedureName = request.ProcedureType?.Name ?? "Trámite",
                CurrentStatus = request.ProcedureFlow?.ProcedureStatus?.Name ?? "Solicitud",
                UserMessage = request.Message ?? "Sin mensaje.",
                CreationDate = request.DateCreated,
                LastUpdate = request.DateUpdated,
                TerminatedDate = request.DateTerminated,
                ProcedureFlow = request.ProcedureFlow,

                Documents = request.ProcedureDocuments.Select(d => new DocumentDetail
                {
                    FileName = d.Name,
                    FilePath = d.FilePath
                }).ToList(),

                History = request.ProcedureMonitorings.Select(m => new MonitoringStep
                {
                    StatusName = m.ProcedureFlow?.ProcedureStatus?.Name ?? "Estatus",
                    AdminComment = m.Comment ?? "Sin comentario",
                    Date = m.DateUpdated,
                    UpdatedBy = "Administración",
                    BackgroundColor = m.ProcedureFlow?.ProcedureStatus?.BackgroundColor ?? "#6c757d",
                    TextColor = m.ProcedureFlow?.ProcedureStatus?.TextColor ?? "#ffffff"
                }).OrderByDescending(h => h.Date).ToList()
            };

            int failureThreshold = 90;

            viewModel.FullProgressSteps = await _context.ProcedureFlow
                .Include(f => f.ProcedureStatus)
                .Where(f => f.IdTypeProcedure == request.IdTypeProcedure)
                .Where(f =>
                    f.StepOrder < failureThreshold ||
                    f.Id == request.IdProcedureFlow
                )
                .OrderBy(f => f.StepOrder)
                .ToListAsync();

            return PartialView("_TrackingDetails", viewModel);
        }

        private async Task<(bool IsAvailable, string Message)> CheckProcedureAvailability(int procedureTypeId)
        {
            var procedure = await _context.ProcedureTypes.FindAsync(procedureTypeId);
            if (procedure == null) return (false, "El trámite no existe.");

            var now = DateTime.Now;

            if (procedure.StartDate.HasValue && now < procedure.StartDate.Value)
            {
                return (false, $"Este trámite aún no está disponible. Abre el {procedure.StartDate.Value:dd/MM/yyyy HH:mm}.");
            }

            if (procedure.EndDate.HasValue && now > procedure.EndDate.Value)
            {
                return (false, "El periodo para este trámite ha finalizado.");
            }

            return (true, "Disponible");
        }

        [AllowAnonymous]
        public async Task<IActionResult> PaymentRegistration()
        {
            bool isAuthenticated = User.Identity?.IsAuthenticated ?? false;

            var now = DateTime.Now;

            if (isAuthenticated)
            {
                ViewBag.TramitesPago = await _context.ProcedureTypes
                    .Where(p => p.Name.Contains(isAuthenticated ? "Inscripción" : "Preinscripción"))
                    .Where(p => (!p.StartDate.HasValue || now >= p.StartDate) &&
                                (!p.EndDate.HasValue || now <= p.EndDate))
                    .ToListAsync();

                var studentData = await _context.PreenrollmentGenerals
                    .Where(p => p.UserId == CurrentUserId)
                    .Select(p => p.Matricula)
                    .FirstOrDefaultAsync();

                ViewBag.UserMatricula = studentData ?? "";
            }
            else
            {
                ViewBag.TramitesPago = await _context.ProcedureTypes
                    .Where(p => p.Name.Contains("Preinscripción"))
                    .ToListAsync();

                ViewBag.UserMatricula = "";
            }

            return View();
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
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

            string yearPrefix = DateTime.Now.ToString("yy");

            var lastRequest = await _context.ProcedureRequest
                .Where(r => r.Folio.StartsWith(yearPrefix))
                .OrderByDescending(r => r.Folio)
                .FirstOrDefaultAsync();

            string newFolio;

            if (lastRequest == null)
            {
                newFolio = yearPrefix + "000001";
            }
            else
            {
                if (int.TryParse(lastRequest.Folio.Substring(2), out int lastNumber))
                {
                    newFolio = yearPrefix + (lastNumber + 1).ToString("D6");
                }
                else
                {
                    newFolio = yearPrefix + "000001";
                }
            }

            var request = new procedure_request
            {
                IdTypeProcedure = procedureTypeId,
                IdProcedureFlow = flow.Id,
                IdUser = student.UserId,
                DateCreated = DateTime.Now,
                Subject = "Validación de pago institucional",
                Message = $"Validación de pago institucional para folio/matrícula: {identifier}",
                Folio = newFolio
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
                        string extension = Path.GetExtension(file.FileName);
                        string fileUrl = await _storageService.UploadFileAsync(file, "proceduresfiles", STORAGE_CONNECTION);
                        _context.ProcedureDocuments.Add(new procedure_documents
                        {
                            IdProcedure = request.Id,
                            Name = $"Voucher_{identifier}{extension}",
                            FilePath = fileUrl,
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
        
        [Authorize(Roles = "Student")]
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