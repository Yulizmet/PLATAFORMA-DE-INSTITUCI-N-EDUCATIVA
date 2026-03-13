namespace SchoolManager.Areas.Grades.ViewModels.Groups
{
    public class GroupDetailsViewModel
    {
        public int GroupId { get; set; }
        public string Name { get; set; } = null!;
        public int GradeLevelId { get; set; }
        public string GradeLevelName { get; set; } = null!;
        public List<GroupSubjectViewModel> Subjects { get; set; } = new();
        public int StudentsCount { get; set; }
        public List<GroupStudentViewModel> Students { get; set; } = new();
    }

    public class GroupStudentViewModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string FullName { get; set; } = null!;
        public string Matricula { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class GroupSubjectViewModel
    {
        public int TeacherSubjectGroupId { get; set; }
        public string SubjectName { get; set; } = null!;
        public string TeacherName { get; set; } = null!;
    }
}