using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class tutorship_monitoring
    {
        [Key]
        public int MonitoringId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public string PerformanceLevel { get; set; } = null!;

        public string DetailedObservations { get; set; } = null!;

        public string ActionPlan { get; set; } = null!;

        public string FilePath { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual users_user Student { get; set; } = null!;

        [ForeignKey("TeacherId")]
        public virtual users_user Teacher { get; set; } = null!;
    }
}