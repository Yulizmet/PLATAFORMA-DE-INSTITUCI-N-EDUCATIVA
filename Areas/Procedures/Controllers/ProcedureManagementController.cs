using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class ProcedureManagementController : Controller
    {
        private readonly AppDbContext _context;

        public ProcedureManagementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.ProcedureRequest
                .Include(r => r.ProcedureDocuments)
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureFlow).ThenInclude(f => f.ProcedureStatus)
                .Include(r => r.Preenrollments)
                .Include(r => r.User)
                    .ThenInclude(u => u.Person)
                .Include(r => r.User)
                    .ThenInclude(u => u.Preenrollments)
                .OrderByDescending(r => r.DateUpdated)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> Monitoring(int id)
        {
            var request = await _context.ProcedureRequest
                .Include(r => r.ProcedureType)
                .Include(r => r.ProcedureFlow).ThenInclude(f => f.ProcedureStatus)
                .Include(r => r.ProcedureDocuments)
                .Include(r => r.User).ThenInclude(u => u.Person)
                .Include(r => r.User).ThenInclude(u => u.Preenrollments)
                .Include(r => r.Preenrollments).ThenInclude(p => p.User).ThenInclude(u => u!.Person)
                .Include(r => r.ProcedureMonitorings).ThenInclude(m => m.ProcedureFlow).ThenInclude(f => f.ProcedureStatus)
                .Include(r => r.ProcedureMonitorings).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            var currentPreenrollment = request.Preenrollments.FirstOrDefault();
            var globalMatricula = currentPreenrollment?.Matricula ??
                                  request.User?.Preenrollments?.FirstOrDefault(p => p.Matricula != "0")?.Matricula;

            var viewModel = new ExtraInfoViewModel
            {
                RequestId = request.Id,
                Folio = request.Folio,
                ProcedureName = request.ProcedureType?.Name ?? "Trámite",
                CurrentStatus = request.ProcedureFlow?.ProcedureStatus?.Name ?? "N/A",
                UserMessage = request.Message,
                CreationDate = request.DateCreated,
                LastUpdate = request.DateUpdated,
                TerminatedDate = request.DateTerminated,

                StudentFullName = currentPreenrollment?.User?.Person != null
                    ? $"{currentPreenrollment.User.Person.FirstName} {currentPreenrollment.User.Person.LastNamePaternal} {currentPreenrollment.User.Person.LastNameMaternal}"
                    : (request.User?.Person != null
                        ? $"{request.User.Person.FirstName} {request.User.Person.LastNamePaternal} {request.User.Person.LastNameMaternal}"
                        : "Usuario sin expediente"),

                Matricula = (globalMatricula != null && globalMatricula != "0") ? globalMatricula : "S/M",
                Email = request.User?.Person?.Email ?? request.User?.Email ?? "N/A",
                ApplicantFolio = currentPreenrollment?.Folio ?? "N/A",
                IsAspirante = request.User == null || (globalMatricula == "0" || globalMatricula == null),
                Documents = request.ProcedureDocuments.Select(d => new DocumentDetail { FileName = d.Name, FilePath = d.FilePath }).ToList()
            };

            var historyList = request.ProcedureMonitorings.Select(m => new MonitoringStep
            {
                StatusName = m.ProcedureFlow?.ProcedureStatus?.Name ?? "Cambio de Estado",
                AdminComment = m.Comment,
                Date = m.DateUpdated,
                UpdatedBy = m.User?.Username ?? "Admin",
                BackgroundColor = m.ProcedureFlow?.ProcedureStatus?.BackgroundColor ?? "#6c757d",
                TextColor = m.ProcedureFlow?.ProcedureStatus?.TextColor ?? "#ffffff"
            }).ToList();

            var flujoInicial = await _context.ProcedureFlow
                .Include(f => f.ProcedureStatus)
                .Where(f => f.IdTypeProcedure == request.IdTypeProcedure)
                .OrderBy(f => f.StepOrder)
                .FirstOrDefaultAsync();

            historyList.Add(new MonitoringStep
            {
                StatusName = flujoInicial?.ProcedureStatus?.Name ?? "Solicitud Iniciada",
                AdminComment = "El usuario ha registrado la solicitud exitosamente.",
                Date = request.DateCreated,
                UpdatedBy = "Sistema",
                BackgroundColor = flujoInicial?.ProcedureStatus?.BackgroundColor ?? "#17a2b8",
                TextColor = flujoInicial?.ProcedureStatus?.TextColor ?? "#ffffff"
            });

            viewModel.History = historyList.OrderByDescending(h => h.Date).ToList();

            var steps = await _context.ProcedureFlow
                .Include(f => f.ProcedureStatus)
                .Where(f => f.IdTypeProcedure == request.IdTypeProcedure && f.ProcedureStatus.Name != "Cancelado")
                .OrderBy(f => f.StepOrder)
                .ToListAsync();

            var selectListItems = steps.Select(f => new {
                Id = f.Id,
                DisplayName = f.StepOrder >= 90 ? $"🔴 {f.ProcedureStatus.Name}" : $"🟢 {f.ProcedureStatus.Name}"
            });

            ViewBag.Statuses = new SelectList(selectListItems, "Id", "DisplayName", request.IdProcedureFlow);

            return PartialView("_Monitoring", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int requestId, int newFlowId, string adminComment)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var request = await _context.ProcedureRequest
                    .Include(r => r.ProcedureType)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null) return Json(new { success = false, message = "Solicitud no encontrada" });

                var newFlowStep = await _context.ProcedureFlow
                    .Include(f => f.ProcedureStatus)
                    .FirstOrDefaultAsync(f => f.Id == newFlowId);

                if (newFlowStep == null) return Json(new { success = false, message = "Estado de flujo no válido" });

                request.IdProcedureFlow = newFlowId; 
                request.DateUpdated = DateTime.Now;

                bool isFinalStatus = newFlowStep.StepOrder >= 90 ||
                                     newFlowStep.ProcedureStatus.Name.ToLower().Contains("aprobado") ||
                                     newFlowStep.ProcedureStatus.Name.ToLower().Contains("finalizado");

                if (isFinalStatus)
                {
                    request.DateTerminated = DateTime.Now;
                }
                else
                {
                    request.DateTerminated = null;
                }

                var monitoring = new procedure_monitoring
                {
                    IdProcedure = requestId,
                    IdProcedureFlow = newFlowId,
                    Comment = adminComment,
                    DateUpdated = DateTime.Now,
                    IdUser = 3 // Recuerda cambiar esto por el ID del usuario logueado después
                };

                _context.ProcedureMonitoring.Add(monitoring);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = isFinalStatus ? "Trámite finalizado con éxito" : "Estatus actualizado correctamente"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}