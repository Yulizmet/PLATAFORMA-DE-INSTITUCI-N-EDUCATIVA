using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace SchoolManager.Models
{
    public class grades_subjects
    {
        public int SubjectId { get; set; }
        public string Name { get; set; } = null!;

        public int GradeLevelId { get; set; }
        [Precision(3, 2)]
        public decimal? MinPassingGrade { get; set; } // Si es null, usa la del nivel
        public grades_grade_level GradeLevel { get; set; } = null!;
        public ICollection<grades_subject_unit> Units { get; set; } = new List<grades_subject_unit>();
    }
}
