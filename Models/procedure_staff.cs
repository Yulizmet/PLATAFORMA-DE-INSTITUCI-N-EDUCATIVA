using SchoolManager.Areas.Procedures.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("procedure_staff")]
    public class procedure_staff
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdUser { get; set; }

        [Required]
        public int IdArea { get; set; }

        [Required]
        public int IdJobPosition { get; set; }

        public bool IsSuperAdmin { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        [ForeignKey("IdUser")]
        public virtual users_user User { get; set; } = null!;

        [ForeignKey("IdJobPosition")]
        public virtual procedure_job_position ProcedureJobPosition { get; set; } = null!;

        [ForeignKey("IdArea")]
        public virtual procedure_areas ProcedureArea { get; set; } = null!;
    }
}