using System.Text.RegularExpressions;

namespace SchoolManager.Models
{
    public class grades_teacher_subject_group
    {
        public int TeacherSubjectGroupId { get; set; }

        public int TeacherSubjectId { get; set; }
        public int GroupId { get; set; }

        public grades_teacher_subject TeacherSubject { get; set; } = null!;
        //public grades_group grades_group { get; set; } = null!;
    }
}
