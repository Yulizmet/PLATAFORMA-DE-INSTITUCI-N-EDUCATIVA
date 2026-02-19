using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class grades_grade_level
    {
        public int GradeLevelId { get; set; }

        [Required]
        public string Name { get; set; } = null!; // "1er Semestre", "3er Semestre"

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsOpen { get; set; }

        // Relaciones
        public ICollection<grades_group> Groups { get; set; } = new List<grades_group>();
        public ICollection<grades_subjects> Subjects { get; set; } = new List<grades_subjects>();
    }
}
