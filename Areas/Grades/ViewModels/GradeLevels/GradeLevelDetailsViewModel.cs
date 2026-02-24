namespace SchoolManager.Areas.Grades.ViewModels.GradeLevels
{
    public class GradeLevelDetailsViewModel
    {
        public int GradeLevelId { get; set; }
        public string Name { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsOpen { get; set; }
        public List<GroupSimpleViewModel> Groups { get; set; } = new();
        public List<SubjectSimpleViewModel> Subjects { get; set; } = new();
    }
}
