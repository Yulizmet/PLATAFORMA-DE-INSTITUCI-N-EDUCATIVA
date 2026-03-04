using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Areas.UserMng.ViewModels;

public class EditStudentVM
{
    [Required]
    public int UserId { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string Username { get; set; }

    public string? Password { get; set; }
}