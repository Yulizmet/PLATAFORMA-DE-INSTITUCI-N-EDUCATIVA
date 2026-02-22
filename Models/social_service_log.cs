using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class social_service_log
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Week { get; set; } = null!;

        [Required]
        public string Activities { get; set; } = null!;

        public int HoursPracticas { get; set; }

        public int HoursServicioSocial { get; set; }

        public string? Observations { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("StudentId")]
        public virtual users_user? Student { get; set; }
    }
}