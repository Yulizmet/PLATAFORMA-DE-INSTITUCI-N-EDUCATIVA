namespace SchoolManager.Areas.Grades.ViewModels.TeacherSubject
{
    public class TeacherSubjectFilterViewModel
    {
        public int? TeacherId { get; set; }
        public int? SubjectId { get; set; }
        public List<TeacherOption> Teachers { get; set; } = new();
        public List<SubjectOption> Subjects { get; set; } = new();
    }

    public class TeacherOption
    {
        public int TeacherId { get; set; }
        public string FullName { get; set; } = null!;
    }

    public class SubjectOption
    {
        public int SubjectId { get; set; }
        public string Name { get; set; } = null!;
        public string GradeLevelName { get; set; } = null!;
    }
}