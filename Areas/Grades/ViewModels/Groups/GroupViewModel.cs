using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Areas.Grades.ViewModels.Groups
{
    public class GroupViewModel
    {
        public int GroupId { get; set; }

        [Required(ErrorMessage = "El nombre del grupo es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre del grupo")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Debes seleccionar un nivel escolar")]
        [Display(Name = "Nivel escolar")]
        public int GradeLevelId { get; set; }

        // Propiedades para la vista
        public string? GradeLevelName { get; set; }
        public int SubjectsCount { get; set; }
        public int StudentsCount { get; set; } // Para futuro
    }
}