namespace SchoolManager.Areas.Grades.ViewModels.GradeLevels
{
    public class GroupSimpleViewModel
    {
        public int GroupId { get; set; }
        public string Name { get; set; } = null!;
        public int SubjectsCount { get; set; }
    }
}
