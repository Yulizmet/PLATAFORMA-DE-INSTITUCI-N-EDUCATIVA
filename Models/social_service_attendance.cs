using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("social_service_attendance")]
    public class social_service_attendance
    {
        [Key]
        [Column("AttendanceId")]
        public int AttendanceId { get; set; }

        [Required]
        [Column("StudentId")]
        public int StudentId { get; set; }

        [Required]
        [Column("Date")]
        public DateTime Date { get; set; }

        [Required]
        [Column("IsPresent")]
        public bool IsPresent { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Tipo")]
        public string Tipo { get; set; } = "Servicio Social";

        [Column("Notes")]
        public string? Notes { get; set; }

        [ForeignKey("StudentId")]
        public virtual users_user? Student { get; set; }
    }
}