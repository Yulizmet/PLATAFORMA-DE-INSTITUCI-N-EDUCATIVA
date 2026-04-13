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
        [Display(Name = "Nombre del trámite")]
        public string Name { get; set; } = null!;

        [Required]
        [Display(Name = "Última modificación")]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Área responsable")]
        public int IdArea { get; set; }

        public DateTime? StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = DateTime.Now;
        public int? MaxResolutionDays { get; set; }

        [ForeignKey("IdArea")]
        public virtual procedure_areas ProcedureArea { get; set; } = null!;

        public virtual ICollection<procedure_flow> ProcedureFlow { get; set; } = new List<procedure_flow>();

        public virtual ICollection<procedure_type_requirements> Requirements { get; set; } = new List<procedure_type_requirements>();

        public virtual ICollection<procedure_request> ProcedureRequests { get; set; } = new List<procedure_request>();
    }
}
