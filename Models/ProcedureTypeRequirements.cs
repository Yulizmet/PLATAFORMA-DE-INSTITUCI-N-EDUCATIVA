using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class ProcedureTypeRequirements
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
        public virtual ProcedureTypes ProcedureType { get; set; } = null!;

        [ForeignKey("IdTypeDocument")]
        public virtual ProcedureTypeDocuments ProcedureTypeDocument { get; set; } = null!;
    }
}
