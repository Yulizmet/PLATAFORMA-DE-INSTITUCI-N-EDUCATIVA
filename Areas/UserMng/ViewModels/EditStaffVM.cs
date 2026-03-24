using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Areas.UserMng.ViewModels;

public class EditStaffVM
{
    [Required]
    public int UserId { get; set; }

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

    public string? Password { get; set; }
}