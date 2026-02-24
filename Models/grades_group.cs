using System.Diagnostics;

namespace SchoolManager.Models
{
    public class grades_group
    {
        public int GroupId { get; set; }
        public string Name { get; set; } = null!; // "101", "A", "B"

        // Foreign keys
        public int GradeLevelId { get; set; }

        // Navigation properties
        public grades_grade_level GradeLevel { get; set; } = null!;

        // Collections
        public ICollection<grades_teacher_subject_group> TeacherSubjectGroups { get; set; } = new List<grades_teacher_subject_group>();
        public ICollection<grades_final_grades> FinalGrades { get; set; } = new List<grades_final_grades>();
        public ICollection<grades_grades> Grades { get; set; } = new List<grades_grades>();
        public ICollection<grades_enrollment> Enrollments { get; set; } = new List<grades_enrollment>();

    }
}
