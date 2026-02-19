using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class procedure_type_requirements
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdTypeProcedure { get; set; }

        [Required]
        public int IdTypeDocument { get; set; }

        [Required]
        public bool IsRequired { get; set; }

        [ForeignKey("IdTypeProcedure")]
        public virtual procedure_types ProcedureType { get; set; } = null!;

        [ForeignKey("IdTypeDocument")]
        public virtual procedure_type_documents ProcedureTypeDocument { get; set; } = null!;
    }
}
