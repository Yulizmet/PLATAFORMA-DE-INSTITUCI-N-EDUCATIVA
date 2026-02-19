namespace SchoolManager.Models
{
    public class grades_teacher_subject
    {
        public int TeacherSubjectId { get; set; }

        public int TeacherId { get; set; } // UserId
        public int SubjectId { get; set; }

        public grades_subjects Subject { get; set; } = null!;
    }
}
