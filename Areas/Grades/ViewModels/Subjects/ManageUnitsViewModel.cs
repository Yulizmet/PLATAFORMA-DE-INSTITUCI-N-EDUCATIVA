namespace SchoolManager.Areas.Grades.ViewModels.Subjects
{
    public class ManageUnitsViewModel
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = null!;
        public string GradeLevelName { get; set; } = null!;
        public List<UnitViewModel> Units { get; set; } = new();
    }
}
