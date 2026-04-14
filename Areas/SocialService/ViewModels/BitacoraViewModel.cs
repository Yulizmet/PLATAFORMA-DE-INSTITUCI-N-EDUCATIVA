using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SchoolManager.Areas.SocialService.ViewModels
{
    public class BitacoraViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "La semana es obligatoria")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Solo se permiten números")]
        [Display(Name = "Semana")]
        public string Week { get; set; }

        [Required(ErrorMessage = "Las actividades son obligatorias")]
        [Display(Name = "Actividades")]
        public string Activities { get; set; }

        [Required(ErrorMessage = "Las horas de prácticas son obligatorias")]
        [Range(0, 40, ErrorMessage = "El valor de {0} debe estar entre {1} y {2}.")]
        [Display(Name = "Horas Prácticas")]
        public int HoursPracticas { get; set; }

        [Required(ErrorMessage = "Las horas de servicio social son obligatorias")]
        [Range(0, 40, ErrorMessage = "El valor de {0} debe estar entre {1} y {2}.")]
        [Display(Name = "Horas Servicio Social")]
        public int HoursServicioSocial { get; set; }

        [Display(Name = "Observaciones")]
        public string? Observations { get; set; }

        [Display(Name = "Archivo PDF")]
        public IFormFile? PdfFile { get; set; }

        public string? ExistingPdfFileName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HoursPracticas == 0 && HoursServicioSocial == 0)
            {
                yield return new ValidationResult(
                    "Debes capturar horas en Prácticas Profesionales o en Servicio Social.");
            }
        }
    }
}