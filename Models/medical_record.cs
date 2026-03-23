using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("medical_records")]
    public class medical_record
    {
        [Key]
        [Column("record_id")]
        public int RecordId { get; set; }

        [Column("student_id")]
        public int? StudentId { get; set; }

        [Column("staff_id")]
        public int? StaffId { get; set; }

        [Column("record_datetime")]
        public DateTime? RecordDatetime { get; set; }

        [Column("consultation_reason")]
        public string? ConsultationReason { get; set; }

        [Column("vital_signs")]
        public string? VitalSigns { get; set; }

        [Column("observations")]
        public string? Observations { get; set; }

        [Column("treatment_action")]
        public string? TreatmentAction { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string? Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("fol")]
        [MaxLength(50)]
        public string? Folio { get; set; }

        [ForeignKey("StudentId")]
        public virtual users_user? Student { get; set; }
    }
}
