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
        [Display(Name = "Fecha de Movimiento")]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        [Required]
        public int IdUser { get; set; }

        [Required]
        public int IdProcedure { get; set; }

        [Required]
        public int IdProcedureFlow { get; set; }

        [ForeignKey("IdUser")]
        public virtual users_user User { get; set; } = null!;

        [ForeignKey("IdProcedure")]
        public virtual procedure_request ProcedureRequest { get; set; } = null!;

        [ForeignKey("IdProcedureFlow")]
        public virtual procedure_flow ProcedureFlow { get; set; } = null!;
    }
}