using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class procedure_request
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Folio { get; set; } = null!;

        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        [Required]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        [Required]
        public int IdUser { get; set; }

        [Required]
        public int IdTypeProcedure { get; set; }

        [Required]
        public int IdProcedureFlow { get; set; }

        [ForeignKey("IdUser")]
        public virtual users_user User { get; set; } = null!;

        [ForeignKey("IdTypeProcedure")]
        public virtual procedure_types ProcedureType { get; set; } = null!;

        [ForeignKey("IdProcedureFlow")]
        public virtual procedure_flow ProcedureFlow { get; set; } = null!;

        public virtual ICollection<preenrollment_general> Preenrollments { get; set; } = new List<preenrollment_general>();

        public virtual ICollection<procedure_documents> ProcedureDocuments { get; set; } = new List<procedure_documents>();
        public virtual ICollection<procedure_monitoring> ProcedureMonitorings { get; set; } = new List<procedure_monitoring>();
    }
}