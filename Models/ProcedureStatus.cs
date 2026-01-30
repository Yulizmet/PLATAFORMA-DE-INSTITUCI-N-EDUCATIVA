using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class ProcedureStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(7)]
        public string Color { get; set; } = null!;

        public virtual ICollection<ProcedureRequest> ProcedureRequests { get; set; } = new List<ProcedureRequest>();
    }
}
