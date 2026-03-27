using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable disable


namespace SchoolManager.Models
{
    [Table("medical_records", Schema = "dbo")]
    public class medical_logbook
    {
        [Key]
        [Column("record_id")]
        public int Id { get; set; }

        [Column("fol")]
        public string Folio { get; set; }

        [Column("student_id")]
        public int IdAlumno { get; set; }

        [Column("staff_id")]
        public int? IdPersonal { get; set; }

        [Column("record_datetime")]
        public DateTime FechaHora { get; set; }

        [Column("consultation_reason")]
        public string MotivoConsulta { get; set; }

        [Column("vital_signs")]
        public string SignosVitales { get; set; }

        [Column("observations")]
        public string Observaciones { get; set; }

        [Column("treatment_action")]
        public string Tratamiento { get; set; }

        [Column("status")]
        public string Estado { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("IdAlumno")]
        public medical_student Alumno { get; set; }
        [NotMapped]
        public string MatriculaTemp { get; set; }
        [NotMapped]
        public string NombreCompletoTemp { get; set; } = "";
    }
}