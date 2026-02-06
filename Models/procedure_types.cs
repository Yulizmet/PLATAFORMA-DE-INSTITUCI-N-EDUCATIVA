using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class procedure_types
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [Required]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        [Required]
        public int IdArea { get; set; }

        [ForeignKey("IdArea")]
        public virtual procedure_areas ProcedureArea { get; set; } = null!;

        public virtual ICollection<procedure_type_requirements> Requirements { get; set; } = new List<procedure_type_requirements>();

        public virtual ICollection<procedure_request> ProcedureRequests { get; set; } = new List<procedure_request>();
    }
}
