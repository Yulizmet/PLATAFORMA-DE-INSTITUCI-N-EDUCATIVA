namespace SchoolManager.Models
{
    public class grades_teacher_subject
    {
        public int TeacherSubjectId { get; set; }

        public int TeacherId { get; set; } // UserId
        public int SubjectId { get; set; }

        public users_user Teacher { get; set; } = null!;
        public grades_subjects Subject { get; set; } = null!;

        public ICollection<grades_teacher_subject_group> TeacherSubjectGroups { get; set; } = new List<grades_teacher_subject_group>();
    }
}