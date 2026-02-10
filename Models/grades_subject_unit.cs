namespace SchoolManager.Models
{
    public class grades_subject_unit
    {
        public int UnitId { get; set; }

        public int SubjectId { get; set; }
        public int UnitNumber { get; set; }
        public bool IsOpen { get; set; }

        public grades_subjects Subject { get; set; } = null!;
    }
}
