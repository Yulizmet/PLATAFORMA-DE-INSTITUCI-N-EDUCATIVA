using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace SchoolManager.Models
{
    public class grades_grades
    {
        public int GradeId { get; set; }

        public int StudentId { get; set; } // UserId
        public int GroupId { get; set; }
        public int SubjectUnitId { get; set; }
        [Precision(5, 2)]
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }

        public grades_group grades_group { get; set; } = null!;
        public grades_subject_unit SubjectUnit { get; set; } = null!;

        public ICollection<grades_unit_recovery> Recoveries { get; set; } = new List<grades_unit_recovery>();
    }
}
