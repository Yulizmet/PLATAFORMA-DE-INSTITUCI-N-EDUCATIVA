using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Areas.Grades.ViewModels.TeacherSubject
{
    public class TeacherSubjectViewModel
    {
        public int TeacherSubjectId { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un profesor")]
        [Display(Name = "Profesor")]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "Debes seleccionar una materia")]
        [Display(Name = "Materia")]
        public int SubjectId { get; set; }

        // Propiedades para mostrar
        public string? TeacherName { get; set; }
        public string? SubjectName { get; set; }
        public string? GradeLevelName { get; set; }
        public int GroupsCount { get; set; }
    }
}