//using System.ComponentModel.DataAnnotations;

//namespace SchoolManager.Areas.Grades.ViewModels.Subjects
//{
//    public class SubjectViewModel
//    {
//        public int SubjectId { get; set; }

//        [Required(ErrorMessage = "El nombre de la materia es requerido")]
//        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
//        [Display(Name = "Nombre de la materia")]
//        public string Name { get; set; } = null!;

//        [Required(ErrorMessage = "Debes seleccionar un nivel escolar")]
//        [Display(Name = "Nivel escolar")]
//        public int GradeLevelId { get; set; }

//        // Propiedades para la vista
//        [Display(Name = "Calificación mínima (opcional)")]
//        public decimal? MinPassingGrade { get; set; }
//        public string? GradeLevelName { get; set; }
//        public int UnitsCount { get; set; }
//        public int OpenUnitsCount { get; set; }
//    }
//}
