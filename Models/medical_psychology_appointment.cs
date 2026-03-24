using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("medical_psychology_appointments")]
    public class medical_psychology_appointment
    {
        [Key]
        [Column("appointment_id")]
        public int AppointmentId { get; set; }

        [Column("preenrollment_id")]
        public int? PreenrollmentId { get; set; }

        [Column("staff_id")]
        public int? StaffId { get; set; }

        [Column("appointment_datetime")]
        public DateTime? AppointmentDatetime { get; set; }

        [Column("attendance_status")]
        [MaxLength(50)]
        public string? AttendanceStatus { get; set; }

        [Column("psychology_observations")]
        public string? PsychologyObservations { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("fol")]
        [MaxLength(50)]
        public string? Folio { get; set; }

        [ForeignKey("PreenrollmentId")]
        public virtual preenrollment_general? PreenrollmentGeneral { get; set; }
    }
}
