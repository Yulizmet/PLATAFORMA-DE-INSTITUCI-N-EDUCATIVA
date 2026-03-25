using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable disable

namespace SchoolManager.Models
{
    [Table("medical_students", Schema = "dbo")]
    public class medical_student
    {
        [Key]
        [Column("student_id")]
        public int Id { get; set; }

        [Column("preenrollment_id")]
        public int PreenrollmentId { get; set; }

        [Column("weight")]
        public decimal? Peso { get; set; }

        [Column("allergies")]
        public string Alergias { get; set; }

        [Column("chronic_conditions")]
        public string CondicionesCronicas { get; set; }

        [Column("created_at")]
        public DateTime FechaCreacion { get; set; }
    }
}