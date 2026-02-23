namespace SchoolManager.Areas.Grades.ViewModels.Groups
{
    public class GroupDetailsViewModel
    {
        public int GroupId { get; set; }
        public string Name { get; set; } = null!;
        public int GradeLevelId { get; set; }
        public string GradeLevelName { get; set; } = null!;
        public List<GroupSubjectViewModel> Subjects { get; set; } = new();
    }

    public class GroupSubjectViewModel
    {
        public int TeacherSubjectGroupId { get; set; }
        public string SubjectName { get; set; } = null!;
        public string TeacherName { get; set; } = null!;
    }
}