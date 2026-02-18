using SchoolManager.Models;
using System.ComponentModel.DataAnnotations;

public class procedure_flow
{
    [Key]
    public int Id { get; set; }

    public int IdTypeProcedure { get; set; }

    public int IdStatus { get; set; }

    public int StepOrder { get; set; }

    public virtual procedure_types ProcedureTypes { get; set; } = null!;
    public virtual procedure_status ProcedureStatus { get; set; } = null!;
}
