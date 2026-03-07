using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Areas.UserMng.ViewModels
{
    public class CreateTeacherVM
    {
        [Required]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; } = null!;

        [Required]
        [Display(Name = "Apellido paterno")]
        public string LastNamePaternal { get; set; } = null!;

        [Required]
        [Display(Name = "Apellido materno")]
        public string LastNameMaternal { get; set; } = null!;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime? BirthDate { get; set; }

        [Required]
        [Display(Name = "Género")]
        public string Gender { get; set; } = null!;

        [Required]
        [Display(Name = "CURP")]
        public string Curp { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "Teléfono")]
        public string Phone { get; set; } = null!;

        [Required]
        [Display(Name = "Usuario")]
        public string Username { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;
    }
}