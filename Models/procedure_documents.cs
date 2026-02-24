using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class procedure_documents
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "Nombre del documento")]
        public string Name { get; set; } = null!;

        [Required]
        public string FilePath { get; set; } = null!;

        [Required]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        [Required]
        public int IdProcedure { get; set; }

        [ForeignKey("IdProcedure")]
        public virtual procedure_request ProcedureRequest { get; set; } = null!;
    }
}
