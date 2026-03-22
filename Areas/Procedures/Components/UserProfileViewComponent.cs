using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SchoolManager.Data;
using System.Security.Claims;

public class UserProfileViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public UserProfileViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userIdClaim = ((ClaimsPrincipal)User).FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Content("");

        int userId = int.Parse(userIdClaim);

        var userData = await _context.Users
            .Include(u => u.Person)
            .Select(u => new {
                u.UserId,
                FirstName = u.Person.FirstName,
                LastName = u.Person.LastNamePaternal,
                JobName = _context.ProcedureStaff
                    .Include(s => s.ProcedureJobPosition)
                    .Where(s => s.IdUser == u.UserId && s.IsActive)
                    .Select(s => s.ProcedureJobPosition.Name)
                    .FirstOrDefault() ?? "Usuario"
            })
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return View(userData);
    }
}