using System.Diagnostics;

namespace SchoolManager.Models
{
    public class grades_group
    {
        public int GroupId { get; set; }
        public string Name { get; set; } = null!;

        public int GradeLevelId { get; set; }
        public int SchoolCycleId { get; set; }

        public grades_grade_level GradeLevel { get; set; } = null!;
        public grades_school_cycle SchoolCycle { get; set; } = null!;
    }
}
