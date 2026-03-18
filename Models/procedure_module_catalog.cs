using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Areas.Procedures.Models
{
    [Table("procedure_module_catalog")]
    public class procedure_module_catalog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ModuleName { get; set; } = null!;

        [StringLength(100)]
        public string? ButtonName { get; set; }

        [StringLength(255)]
        public string? Route { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}