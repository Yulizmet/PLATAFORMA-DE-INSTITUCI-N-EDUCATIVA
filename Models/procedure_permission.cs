using SchoolManager.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Areas.Procedures.Models
{
    [Table("procedure_permission")]
    public class procedure_permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdArea { get; set; }

        [Required]
        public int IdJobPosition { get; set; }

        [Required]
        public int IdModuleCatalog { get; set; }

        public bool CanView { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("IdArea")]
        public virtual procedure_areas? Area { get; set; }

        [ForeignKey("IdJobPosition")]
        public virtual procedure_job_position? JobPosition { get; set; }

        [ForeignKey("IdModuleCatalog")]
        public virtual procedure_module_catalog? ModuleCatalog { get; set; }
    }
}