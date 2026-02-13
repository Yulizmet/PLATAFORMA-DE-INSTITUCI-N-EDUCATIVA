namespace SchoolManager.Models
{
    public class grades_grade_level
    {
        public int GradeLevelId { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<grades_group> Groups { get; set; } = new List<grades_group>();
        public ICollection<grades_subjects> Subjects { get; set; } = new List<grades_subjects>();
    }
}
