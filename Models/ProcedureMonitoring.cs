using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class ProcedureMonitoring
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Comment { get; set; } = null!;

        [Required]
        public DateTime Datetime { get; set; } = DateTime.Now;

        [Required]
        public string IdUser { get; set; } = null!;

        [Required]
        public int IdProcedure { get; set; }

        [Required]
        public int IdStatus { get; set; } // El nuevo estado asignado

        [ForeignKey("IdProcedure")]
        public virtual ProcedureRequest ProcedureRequest { get; set; } = null!;

        [ForeignKey("IdStatus")]
        public virtual ProcedureStatus ProcedureStatus { get; set; } = null!;
    }
}
