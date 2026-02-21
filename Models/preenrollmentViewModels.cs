using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models.ViewModels
{
    // =====================================================================
    // VIEWMODEL - FLUJO 1: Registro inicial + generación de folio
    // Combina los campos de preenrollment_general y sus tablas relacionadas
    // =====================================================================
    public class PreEnrollmentCreateViewModel
    {
        // --- General ---
        [Required(ErrorMessage = "Selecciona una carrera.")]
        public int IdCareer { get; set; }

        [Required(ErrorMessage = "Selecciona una generación.")]
        public int IdGeneration { get; set; }

        [MaxLength(10)]
        public string? BloodType { get; set; }

        [Required(ErrorMessage = "El estado civil es obligatorio."), MaxLength(30)]
        public string MaritalStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nacionalidad es obligatoria."), MaxLength(50)]
        public string Nationality { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Occupation { get; set; }

        public bool Work { get; set; }

        [MaxLength(200)]
        public string? WorkAddress { get; set; }

        [MaxLength(20)]
        public string? WorkPhone { get; set; }

        // --- Domicilio (preenrollment_addresses) ---
        [Required(ErrorMessage = "La calle es obligatoria.")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número exterior es obligatorio.")]
        public string ExteriorNumber { get; set; } = string.Empty;

        public string? InteriorNumber { get; set; }

        [Required(ErrorMessage = "El código postal es obligatorio.")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "La colonia es obligatoria.")]
        public string Neighborhood { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado es obligatorio.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Phone { get; set; } = string.Empty;

        // --- Escuela de procedencia (preenrollment_schools) ---
        [Required(ErrorMessage = "El nombre de la escuela es obligatorio.")]
        public string School { get; set; } = string.Empty;

        [Required(ErrorMessage = "El grado es obligatorio.")]
        public string Degree { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado de la escuela es obligatorio.")]
        public string SchoolState { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad de la escuela es obligatoria.")]
        public string SchoolCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "El promedio es obligatorio.")]
        [Range(0, 10, ErrorMessage = "El promedio debe estar entre 0 y 10.")]
        public decimal Average { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "El sistema de estudio es obligatorio.")]
        public string StudySystem { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de bachillerato es obligatorio.")]
        public string HighSchoolType { get; set; } = string.Empty;

        // --- Tutor (preenrollment_tutors) ---
        [Required(ErrorMessage = "El parentesco del tutor es obligatorio.")]
        public string TutorRelationship { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno del tutor es obligatorio.")]
        public string TutorPaternalLastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido materno del tutor es obligatorio.")]
        public string TutorMaternalLastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del tutor es obligatorio.")]
        public string TutorName { get; set; } = string.Empty;

        public string? TutorHomePhone { get; set; }
        public string? TutorWorkPhone { get; set; }

        // --- Info adicional (preenrollment_infos) ---
        public string? Beca { get; set; }
        public bool ComuIndi { get; set; }
        public bool LenguIndi { get; set; }
        public bool Incapa { get; set; }
        public bool Disease { get; set; }
        public string? Comment { get; set; }
    }

    // =====================================================================
    // VIEWMODEL - FLUJO 2: Completar registro y crear cuenta
    // Solo pide los datos que faltan: email, contraseña y confirmación.
    // Los demás datos ya están en preenrollment y se pre-llenan en la vista.
    // =====================================================================
    public class CompleteRegistrationViewModel
    {
        public int IdData { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma tu contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}