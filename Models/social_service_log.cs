using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("social_service_log")]
    public class social_service_log
    {
        [Key]
        [Column("LogId")]
        public int LogId { get; set; }

        [Required]
        [Column("StudentId")]
        public int StudentId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Week")]
        public string Week { get; set; } = null!;

        [Required]
        [Column("Activities")]
        public string Activities { get; set; } = null!;

        [Column("HoursPracticas")]
        public int HoursPracticas { get; set; }

        [Column("HoursServicioSocial")]
        public int HoursServicioSocial { get; set; }

        [Column("Observations")]
        public string? Observations { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Campos para la aprobación
        [Column("IsApproved")]
        public bool IsApproved { get; set; } = false;

        [Column("ApprovedHoursPracticas")]
        public int ApprovedHoursPracticas { get; set; } = 0;

        [Column("ApprovedHoursServicioSocial")]
        public int ApprovedHoursServicioSocial { get; set; } = 0;

        [Column("ApprovedBy")]
        public int? ApprovedBy { get; set; }

        [Column("ApprovedAt")]
        public DateTime? ApprovedAt { get; set; }

        [Column("TeacherComments")]
        public string? TeacherComments { get; set; }

        [MaxLength(255)]
        [Column("PdfFileName")]
        public string? PdfFileName { get; set; }

        [ForeignKey("StudentId")]
        public virtual users_user? Student { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual users_user? Approver { get; set; }
    }
}