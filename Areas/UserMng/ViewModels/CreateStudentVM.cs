using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Areas.UserMng.ViewModels;

public class CreateStudentVM
{
    [Required(ErrorMessage = "Debe seleccionar una persona.")]
    public int PersonId { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo no válido.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; }
}