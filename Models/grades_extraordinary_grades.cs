using Microsoft.EntityFrameworkCore;

namespace SchoolManager.Models
{
    public class grades_extraordinary_grades
    {
        public int ExtraordinaryGradeId { get; set; }

        public int FinalGradeId { get; set; }
        [Precision(5, 2)]
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }

        public grades_final_grades FinalGrade { get; set; } = null!;
    }
}
