using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Areas.Procedures.Models
{
    [Table("procedure_job_position")]
    public class procedure_job_position
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}