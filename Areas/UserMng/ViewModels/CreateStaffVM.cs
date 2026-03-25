using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Areas.UserMng.ViewModels;

public class CreateStaffVM
{
    [Required]
    public string RoleName { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastNamePaternal { get; set; }

    public string? LastNameMaternal { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? Curp { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}