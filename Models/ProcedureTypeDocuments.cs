using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class ProcedureTypeDocuments
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(250)]
        public string? Description { get; set; }

        [Required]
        public DateTime Datetime { get; set; } = DateTime.Now;

        public virtual ICollection<ProcedureTypeRequirements> ProcedureTypeRequirements { get; set; } = new List<ProcedureTypeRequirements>();
    }
}
