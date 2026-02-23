// Models/ViewModels/GradeLevelViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Areas.Grades.ViewModels.GradeLevels
{
    public class GradeLevelViewModel
    {
        public int GradeLevelId { get; set; }

        [Required(ErrorMessage = "El nombre del nivel es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        [Display(Name = "Nombre del Nivel")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        public DateOnly EndDate { get; set; }

        [Display(Name = "Nivel Abierto")]
        public bool IsOpen { get; set; }

        // Propiedades para la vista (solo lectura)
        public int GroupsCount { get; set; }
        public int SubjectsCount { get; set; }
    }
}