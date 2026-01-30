using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class ProcedureTypes
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [Required]
        public DateTime Datetime { get; set; } = DateTime.Now;

        [Required]
        public int IdArea { get; set; }

        [ForeignKey("IdArea")]
        public virtual ProcedureAreas ProcedureArea { get; set; } = null!;

        public virtual ICollection<ProcedureTypeRequirements> Requirements { get; set; } = new List<ProcedureTypeRequirements>();

        // Relación con las solicitudes hechas
        public virtual ICollection<ProcedureRequest> ProcedureRequests { get; set; } = new List<ProcedureRequest>();
    }
}
