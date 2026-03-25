using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("medical_psychology_appointments")]
    public class medical_pychology
    {
        [Key]
        [Column("appointment_id")]
        public int Id { get; set; }

        [Column("preenrollment_id")]
        public int PreenrollmentId { get; set; }

        [Column("staff_id")]
        public int? StaffId { get; set; }

        [Column("appointment_datetime")]
        public DateTime AppointmentDatetime { get; set; }

        [Column("attendance_status")]
        public string? AttendanceStatus { get; set; }

        [Column("psychology_observations")]
        public string? PsychologyObservations { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("fol")]
        public string? Fol { get; set; }
        [NotMapped]
        public string? MatriculaTemp { get; set; }
        [NotMapped]
        public string NombreCompletoTemp { get; set; } = "";
    }
}