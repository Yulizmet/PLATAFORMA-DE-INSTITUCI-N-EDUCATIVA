using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class ProcedureDocuments
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [Required]
        public string FilePath { get; set; } = null!;

        [Required]
        public DateTime Datetime { get; set; } = DateTime.Now;

        [Required]
        public int IdProcedure { get; set; }

        [ForeignKey("IdProcedure")]
        public virtual ProcedureRequest ProcedureRequest { get; set; } = null!;
    }
}
