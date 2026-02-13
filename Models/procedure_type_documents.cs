using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class procedure_type_documents
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Display(Name = "Nombre de documento")]
        public string Name { get; set; } = null!;

        [MaxLength(250)]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Última modificación")]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        public virtual ICollection<procedure_type_requirements> ProcedureTypeRequirements { get; set; } = new List<procedure_type_requirements>();
    }
}
