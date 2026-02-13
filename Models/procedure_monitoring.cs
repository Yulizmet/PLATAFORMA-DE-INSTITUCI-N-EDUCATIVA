using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class procedure_monitoring
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Mensaje")]
        public string Comment { get; set; } = null!;

        [Required]
        [Display(Name = "Última modificación")]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        [Required]
        public int IdUser { get; set; }

        [Required]
        public int IdProcedure { get; set; }

        [Required]
        public int IdStatus { get; set; }

        [ForeignKey("IdProcedure")]
        public virtual procedure_request ProcedureRequest { get; set; } = null!;

        [ForeignKey("IdStatus")]
        public virtual procedure_status ProcedureStatus { get; set; } = null!;
    }
}
