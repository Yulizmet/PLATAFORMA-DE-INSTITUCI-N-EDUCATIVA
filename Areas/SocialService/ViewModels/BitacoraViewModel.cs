using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Areas.SocialService.ViewModels
{
    public class BitacoraViewModel
    {
        [Required(ErrorMessage = "La semana es obligatoria")]
        [Display(Name = "Semana")]
        public string Week { get; set; }

        [Required(ErrorMessage = "Las actividades son obligatorias")]
        [Display(Name = "Actividades")]
        public string Activities { get; set; }

        [Required(ErrorMessage = "Las horas de prácticas son obligatorias")]
        [Range(0, 100, ErrorMessage = "Las horas deben estar entre 0 y 100")]
        [Display(Name = "Horas Prácticas")]
        public int HoursPracticas { get; set; }

        [Required(ErrorMessage = "Las horas de servicio social son obligatorias")]
        [Range(0, 100, ErrorMessage = "Las horas deben estar entre 0 y 100")]
        [Display(Name = "Horas Servicio Social")]
        public int HoursServicioSocial { get; set; }

        [Display(Name = "Observaciones")]
        public string? Observations { get; set; } // Opcional - nullable
    }
}