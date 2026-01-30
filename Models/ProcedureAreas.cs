using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class ProcedureAreas
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime Datetime { get; set; } = DateTime.Now;

        public virtual ICollection<ProcedureTypes> ProcedureTypes { get; set; } = new List<ProcedureTypes>();
    }
}
