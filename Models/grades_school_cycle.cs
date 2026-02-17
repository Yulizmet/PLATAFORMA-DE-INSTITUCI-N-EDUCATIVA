using System.Text.RegularExpressions;

namespace SchoolManager.Models
{
    public class grades_school_cycle
    {
        public int SchoolCycleId { get; set; }
        public string Name { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsOpen { get; set; }

        public ICollection<grades_group> Groups { get; set; } = new List<grades_group>();
    }
}
