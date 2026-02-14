using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace SchoolManager.Models
{
    public class grades_final_grades
    {
        public int FinalGradeId { get; set; }

        public int StudentId { get; set; } // UserId
        public int SubjectId { get; set; }
        public int GroupId { get; set; }
        [Precision(5, 2)]
        public decimal Value { get; set; }
        public bool Passed { get; set; }
        public DateTime CreatedAt { get; set; }

        public grades_subjects Subject { get; set; } = null!;
        public grades_group grades_group { get; set; } = null!;

        public grades_extraordinary_grades? ExtraordinaryGrade { get; set; }
    }
}
