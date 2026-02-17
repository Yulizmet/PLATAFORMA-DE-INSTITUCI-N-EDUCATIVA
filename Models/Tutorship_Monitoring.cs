using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class TutorshipMonitoring
    {
        [Key]
        public int MonitoringId { get; set; }
        public int StudentId { get; set; } 
        public int TeacherId { get; set; } 
        public DateTime Date { get; set; }


        [Required(ErrorMessage = "La foto de perfil es obligatoria")]
        public string FilePath { get; set; } = null!;
        public string PerformanceLevel { get; set; } 
        public string DetailedObservations { get; set; } 
        public string ActionPlan { get; set; }
        public string Certificates { get; set; }
    }
}