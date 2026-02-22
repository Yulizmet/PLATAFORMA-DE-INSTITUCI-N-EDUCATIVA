using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class social_service_attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsPresent { get; set; }

        // Indica el tipo de asistencia; por defecto "Servicio Social"
        [Required]
        public string Tipo { get; set; } = "Servicio Social";

        public string? Notes { get; set; }

        [ForeignKey("StudentId")]
        public virtual users_user? Student { get; set; }
    }
}