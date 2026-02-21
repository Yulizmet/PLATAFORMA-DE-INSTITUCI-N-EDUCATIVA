using SchoolManager.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class procedure_flow
    {
        [Key]
        public int Id { get; set; }

        public int IdTypeProcedure { get; set; }

        public int IdStatus { get; set; }

        public int StepOrder { get; set; }

        [ForeignKey("IdTypeProcedure")]
        public virtual procedure_types ProcedureType { get; set; } = null!;

        [ForeignKey("IdStatus")]
        public virtual procedure_status ProcedureStatus { get; set; } = null!;
    }
}