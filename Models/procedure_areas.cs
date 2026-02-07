using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class procedure_areas
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        public virtual ICollection<procedure_types> ProcedureTypes { get; set; } = new List<procedure_types>();
    }
}
