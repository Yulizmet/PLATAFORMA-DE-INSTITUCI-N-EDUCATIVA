using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class procedure_type_documents
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(250)]
        public string? Description { get; set; }

        [Required]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        public virtual ICollection<procedure_type_requirements> ProcedureTypeRequirements { get; set; } = new List<procedure_type_requirements>();
    }
}
