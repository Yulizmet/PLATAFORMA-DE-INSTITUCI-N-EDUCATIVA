using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.ViewModels;
using SchoolManager.Data;
using static SchoolManager.Areas.Procedures.ViewModels.ProceedingDetailViewModel;

[Area("Procedures")]
public class ProceedingController : Controller
{
    private readonly AppDbContext _context;

    public ProceedingController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var expedientes = await _context.Persons
            .Include(p => p.User)
                .ThenInclude(u => u.UserRoles)
            .Include(p => p.User)
                .ThenInclude(u => u.Preenrollments)
                    .ThenInclude(pg => pg.Career)
            .Where(p => p.User != null && p.User.UserRoles.Any(ur => ur.RoleId == 1))
            .Select(p => new StudentExpedienteViewModel
            {
                PersonId = p.PersonId,
                FullName = $"{p.FirstName} {p.LastNamePaternal} {p.LastNameMaternal}",
                Email = p.Email,
                CreatedDate = p.CreatedDate,
                IsActive = p.IsActive,
                Username = p.User.Username,

                CareerName = p.User.Preenrollments.Any()
                             ? p.User.Preenrollments.FirstOrDefault().Career.name_career
                             : "Sin asignar",

                Matricula = p.User.Preenrollments.Any()
                            ? p.User.Preenrollments.FirstOrDefault().Matricula
                            : "0",

                Folio = p.User.Preenrollments.Any()
                        ? p.User.Preenrollments.FirstOrDefault().Folio
                        : "S/F"
            })
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();

        return View(expedientes);
    }

    public async Task<IActionResult> Details(int id)
    {
        var data = await _context.Persons
            .Include(p => p.User)
                .ThenInclude(u => u.Preenrollments)
                    .ThenInclude(pg => pg.Career)
            .Include(p => p.User.Preenrollments)
                .ThenInclude(pg => pg.Generation)
            .Include(p => p.User.Preenrollments)
                .ThenInclude(pg => pg.Addresses)
            .Include(p => p.User.Preenrollments)
                .ThenInclude(pg => pg.Infos)
            .Include(p => p.User.Preenrollments)
                .ThenInclude(pg => pg.Schools)
            .Include(p => p.User.ProcedureRequests)
                .ThenInclude(pr => pr.ProcedureDocuments)
            .FirstOrDefaultAsync(p => p.PersonId == id);

        if (data == null) return NotFound();

        var pre = data.User?.Preenrollments.FirstOrDefault();
        var addr = pre?.Addresses?.FirstOrDefault();
        var info = pre?.Infos?.FirstOrDefault();
        var school = pre?.Schools?.FirstOrDefault();

        var viewModel = new ProceedingDetailViewModel
        {
            FirstName = data.FirstName,
            LastNamePaternal = data.LastNamePaternal,
            LastNameMaternal = data.LastNameMaternal,
            Curp = data.Curp,
            BirthDate = data.BirthDate?.ToString("dd/MM/yyyy") ?? "N/A",
            Gender = data.Gender == "M" ? "Masculino" : "Femenino",
            Nationality = pre?.Nationality ?? "N/A",

            Username = data.User?.Username ?? "Sin usuario",
            UserStatus = data.User != null ? (data.User.IsActive ? "Activo" : "Inactivo") : "N/A",
            LastLogin = data.User?.LastLoginDate?.ToString("dd/MM/yyyy hh:mm tt"),

            Matricula = pre?.Matricula ?? "0",
            Folio = pre?.Folio ?? "N/A",
            CareerName = pre?.Career?.name_career ?? "No asignada",
            Generation = pre?.Generation?.Year.ToString() ?? "N/A",

            BloodType = pre?.BloodType ?? "No registrado",
            MaritalStatus = pre?.MaritalStatus ?? "SOLTERO(A)",
            Occupation = pre?.Occupation ?? "N/A",
            DoesWork = pre?.Work ?? false,
            WorkAddress = pre?.WorkAddress,
            WorkPhone = pre?.WorkPhone,

            Beca = info?.beca ?? "NO",
            IsIndigena = info?.comu_indi ?? false,
            HasIncapacidad = info?.incapa ?? false,
            HasDisease = info?.disease ?? false,
            HealthComments = info?.comment,

            PreviousSchool = school?.school ?? "N/A",
            PreviousAverage = school?.average ?? 0,
            PreviousDegree = school?.degree ?? "N/A",

            Email = data.Email,
            Phone = data.Phone ?? addr?.phone ?? "N/A",

            Street = addr?.street ?? "N/A",
            ExtNum = addr?.exterior_number ?? "S/N",
            IntNum = addr?.interior_number,
            Colony = addr?.neighborhood ?? "N/A",
            ZipCode = addr?.postal_code ?? "N/A",
            CityState = $"{(addr?.city ?? "N/A")}, {(addr?.state ?? "N/A")}"
        };

        //if (data.User?.ProcedureRequests != null)
        //{
        //    viewModel.DigitalFiles = data.User.ProcedureRequests
        //        .SelectMany(pr => pr.ProcedureDocuments)
        //        .Select(d => new ProceedingDetailViewModel.EnrollmentDocumentDetail
        //        {
        //            FileName = d.Name,
        //            FilePath = d.FilePath
        //        }).ToList();
        //}

        return View("Proceeding", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Activate(int? id)
    {
        if (id == null) return NotFound();

        var person = await _context.Persons
            .FirstOrDefaultAsync(m => m.PersonId == id);

        if (person == null) return NotFound();

        return PartialView("_ActivateModal", person);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateConfirmed(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var person = await _context.Persons
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PersonId == id);

            if (person == null)
                return Json(new { success = false, message = "Expediente no encontrado." });

            person.IsActive = true;
            _context.Entry(person).State = EntityState.Modified;

            if (person.User != null)
            {
                person.User.IsActive = true;
                _context.Entry(person.User).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true, message = "El expediente ha sido reactivado con éxito." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { success = false, message = "Error al procesar la alta: " + ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Deactivate(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var person = await _context.Persons
            .FirstOrDefaultAsync(m => m.PersonId == id);

        if (person == null)
        {
            return NotFound();
        }

        return PartialView("_DeactivateModal", person);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateConfirmed(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var person = await _context.Persons
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PersonId == id);

            if (person == null)
                return Json(new { success = false, message = "Expediente no encontrado." });

            person.IsActive = false;
            _context.Entry(person).State = EntityState.Modified;

            if (person.User != null)
            {
                person.User.IsActive = false;
                _context.Entry(person.User).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true, message = "El expediente ha sido dado de baja correctamente." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { success = false, message = "Error al procesar la baja: " + ex.Message });
        }
    }
}