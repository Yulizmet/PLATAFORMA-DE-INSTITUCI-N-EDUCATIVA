using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace SchoolManager.Models
{
    public class grades_unit_recovery
    {
        public int UnitRecoveryId { get; set; }

        public int GradeId { get; set; }
        [Precision(5, 2)]
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }

        public grades_grades Grade { get; set; } = null!;
    }
}
