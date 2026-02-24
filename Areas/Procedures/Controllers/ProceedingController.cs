using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.ViewModels;
using SchoolManager.Data;

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
                .ThenInclude(u => u.Preenrollments)
            .Select(p => new StudentExpedienteViewModel
            {
                PersonId = p.PersonId,
                FullName = $"{p.FirstName} {p.LastNamePaternal} {p.LastNameMaternal}",
                Email = p.Email,
                CreatedDate = p.CreatedDate,
                IsActive = p.IsActive,
                Username = p.User != null ? p.User.Username : "Sin Usuario",

                Matricula = p.User != null && p.User.Preenrollments.Any()
                            ? p.User.Preenrollments.FirstOrDefault().Matricula
                            : "0",

                Folio = p.User != null && p.User.Preenrollments.Any()
                        ? p.User.Preenrollments.FirstOrDefault().Folio
                        : "S/F"
            })
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();

        return View(expedientes);
    }
}