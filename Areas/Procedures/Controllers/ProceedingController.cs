using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;
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

    [HttpGet]
    public IActionResult QuickAddStudent()
    {
        return PartialView("_CreateModal");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreateStudent(string FirstName, string LastNamePaternal, string LastNameMaternal, string Curp, string Email, string Username, string Matricula)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var person = new users_person
            {
                FirstName = FirstName,
                LastNamePaternal = LastNamePaternal,
                LastNameMaternal = LastNameMaternal,
                Curp = Curp,
                Gender = "F",
                Email = Email,
                Phone = "8990000000",
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _context.Persons.Add(person);
            await _context.SaveChangesAsync();

            var user = new users_user
            {
                PersonId = person.PersonId,
                Username = Username.ToLower(),
                Email = Email,
                PasswordHash = "HASH_ESTUDIANTE_TEST",
                IsLocked = false,
                LockReason = "",
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new users_userrole { UserId = user.UserId, RoleId = 1, CreatedDate = DateTime.Now, IsActive = true });

            var pre = new preenrollment_general
            {
                IdCareer = 5,
                IdGeneration = 4,
                UserId = user.UserId,
                BloodType = "O+",
                CreateStat = DateTime.Now,
                Folio = "F-" + new Random().Next(1000, 9999),
                MaritalStatus = "Soltero(a)",
                Nationality = "Mexicana",
                Occupation = "Estudiante",
                Work = false,
                WorkAddress = "N/A",
                Matricula = Matricula
            };
            _context.PreenrollmentGenerals.Add(pre);
            await _context.SaveChangesAsync();

            _context.PreenrollmentAddresses.Add(new preenrollment_addresses
            {
                id_data = pre.IdData,
                street = "Av. Prueba",
                exterior_number = "123",
                interior_number = "N/A",
                postal_code = "88700",
                neighborhood = "Centro",
                state = "Tamaulipas",
                city = "Reynosa",
                phone = "8991234567"
            });

            _context.PreenrollmentInfos.Add(new preenrollment_infos
            {
                id_data = pre.IdData,
                beca = "NINGUNA",
                comu_indi = false,
                lengu_indi = false,
                incapa = false,
                disease = false,
                comment = "Sin comentarios"
            });

            _context.PreenrollmentSchools.Add(new preenrollment_schools
            {
                id_data = pre.IdData,
                school = "CBTIS Test",
                degree = "Bachillerato",
                average = 9.0m,
                city = "Reynosa",
                state = "Tamaulipas",
                high_school_type = "Pública",
                study_system = "Semestral",
                start_date = DateTime.Now.AddYears(-3),
                end_date = DateTime.Now.AddYears(-1)
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true, message = "Expediente completo creado con éxito." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { success = false, message = "Error: " + (ex.InnerException?.Message ?? ex.Message) });
        }
    }

    private ProceedingDetailViewModel MapToViewModel(users_person data)
    {
        var pre = data.User?.Preenrollments.FirstOrDefault();
        var addr = pre?.Addresses?.FirstOrDefault();
        var info = pre?.Infos?.FirstOrDefault();
        var school = pre?.Schools?.FirstOrDefault();

        return new ProceedingDetailViewModel
        {
            PersonId = data.PersonId,
            FirstName = data.FirstName,
            LastNamePaternal = data.LastNamePaternal,
            LastNameMaternal = data.LastNameMaternal,
            Curp = data.Curp,
            Gender = data.Gender == "M" ? "Masculino" : "Femenino",
            Matricula = pre?.Matricula ?? "0",
            BloodType = pre?.BloodType ?? "",
            BirthDate = data.BirthDate?.ToString("yyyy-MM-dd") ?? "",
            MaritalStatus = pre?.MaritalStatus ?? "SOLTERO(A)",
            Beca = info?.beca ?? "NO",
            IsIndigena = info?.comu_indi ?? false,
            HasIncapacidad = info?.incapa ?? false,
            HasDisease = info?.disease ?? false,
            HealthComments = info?.comment,
            PreviousSchool = school?.school ?? "",
            PreviousAverage = school?.average ?? 0,
            PreviousDegree = school?.degree ?? "",
            Email = data.Email,
            Street = addr?.street ?? "",
            ExtNum = addr?.exterior_number ?? "",
            IntNum = addr?.interior_number,
            Colony = addr?.neighborhood ?? "",
            ZipCode = addr?.postal_code ?? "",
            CityState = $"{(addr?.city ?? "")}, {(addr?.state ?? "")}"
        };
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
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

        var viewModel = MapToViewModel(data);

        return View("EditProceeding", viewModel);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProceeding(ProceedingDetailViewModel model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var preEnrollment = await _context.PreenrollmentGenerals
                .Include(p => p.User).ThenInclude(u => u.Person)
                .Include(p => p.Addresses)
                .Include(p => p.Infos)
                .Include(p => p.Schools)
                .FirstOrDefaultAsync(p => p.Matricula == model.Matricula);

            if (preEnrollment == null) return NotFound();

            //await AuditRegistration(preEnrollment.UserId ?? 0, "UPDATE_EXPEDIENTE", "Multiple_Tables");

            var person = preEnrollment.User.Person;
            if (DateTime.TryParse(model.BirthDate, out DateTime fechaNac))
            {
                person.BirthDate = fechaNac;
            }

            preEnrollment.MaritalStatus = !string.IsNullOrEmpty(model.MaritalStatus)
                                          ? model.MaritalStatus
                                          : "SOLTERO(A)";
            var address = preEnrollment.Addresses.FirstOrDefault();
            var info = preEnrollment.Infos.FirstOrDefault();
            var school = preEnrollment.Schools.FirstOrDefault();

            person.FirstName = model.FirstName;
            person.LastNamePaternal = model.LastNamePaternal;
            person.LastNameMaternal = model.LastNameMaternal;
            person.Curp = model.Curp;
            person.Email = model.Email;
            person.Gender = model.Gender == "Masculino" ? "M" : "F";

            preEnrollment.BloodType = model.BloodType;
            preEnrollment.MaritalStatus = model.MaritalStatus;

            if (address != null)
            {
                address.street = model.Street;
                address.exterior_number = model.ExtNum;
                address.interior_number = model.IntNum;
                address.neighborhood = model.Colony;
                address.postal_code = model.ZipCode;
            }

            if (info != null)
            {
                info.beca = model.Beca;
                info.comu_indi = model.IsIndigena;
                info.incapa = model.HasIncapacidad;
                info.disease = model.HasDisease;
                info.comment = model.HealthComments;
            }

            if (school != null)
            {
                school.school = model.PreviousSchool;
                school.degree = model.PreviousDegree;
                school.average = model.PreviousAverage;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true, message = "Expediente actualizado correctamente." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            var innerMessage = ex.InnerException != null
                ? ex.InnerException.Message
                : ex.Message;

            var cleanError = innerMessage.Length > 200
                ? innerMessage.Substring(0, 200) + "..."
                : innerMessage;

            return Json(new { success = false, message = "Error de base de datos: " + cleanError });
        }
    }

    private async Task AuditRegistration(int userId, string accion, string tabla)
    {
        try
        {
            var log = new users_auditlog
            {
                UserId = userId,
                Action = accion,
                TableName = tabla,
                CreatedDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch
        {
        }
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