using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class ProcedureRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Folio { get; set; } = null!;

        [Required]
        public DateTime Datetime { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        // Relaciones
        [Required]
        public string IdUser { get; set; } = null!;

        [Required]
        public int IdTypeProcedure { get; set; }

        [Required]
        public int IdStatus { get; set; }

        [ForeignKey("IdTypeProcedure")]
        public virtual ProcedureTypes ProcedureType { get; set; } = null!;

        [ForeignKey("IdStatus")]
        public virtual ProcedureStatus ProcedureStatus { get; set; } = null!;

        public virtual ICollection<ProcedureDocuments> ProcedureDocuments { get; set; } = new List<ProcedureDocuments>();
        public virtual ICollection<ProcedureMonitoring> ProcedureMonitorings { get; set; } = new List<ProcedureMonitoring>();
    }
}
