namespace SchoolManager.Areas.Grades.ViewModels.Subjects
{
    public class SubjectDetailsViewModel
    {
        public int SubjectId { get; set; }
        public string Name { get; set; } = null!;
        public int GradeLevelId { get; set; }
        public string GradeLevelName { get; set; } = null!;
        public List<UnitViewModel> Units { get; set; } = new();
    }
}
