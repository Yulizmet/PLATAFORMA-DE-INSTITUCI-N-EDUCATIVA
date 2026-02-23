namespace SchoolManager.Areas.Grades.ViewModels.TeacherSubject
{
    public class TeacherAssignmentViewModel
    {
        public int TeacherSubjectId { get; set; }
        public string TeacherName { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public string GradeLevelName { get; set; } = null!;
        public List<GroupAssignmentViewModel> AvailableGroups { get; set; } = new();
    }

    public class GroupAssignmentViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public bool IsAssigned { get; set; }
        public int TeacherSubjectGroupId { get; set; }
    }
}