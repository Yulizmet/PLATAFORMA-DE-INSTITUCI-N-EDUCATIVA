namespace SchoolManager.Areas.Grades.ViewModels.Groups
{
    public class GroupFilterViewModel
    {
        public int? GradeLevelId { get; set; }
        public List<GradeLevelOption> GradeLevels { get; set; } = new();
    }

    public class GradeLevelOption
    {
        public int GradeLevelId { get; set; }
        public string Name { get; set; } = null!;
    }
}