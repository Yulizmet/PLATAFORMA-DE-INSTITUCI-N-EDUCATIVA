using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class procedure_areas
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nombre del área")]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Última modificación")]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        public virtual ICollection<procedure_types> ProcedureTypes { get; set; } = new List<procedure_types>();
    }
}
