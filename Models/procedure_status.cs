using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class procedure_status
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Estado")]
        public string Name { get; set; } = null!;

        [MaxLength(50)]
        public string InternalCode { get; set; } = null!;

        [Required]
        [MaxLength(7)]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Formato hexadecimal inválido")]
        public string BackgroundColor { get; set; } = "#FFFFFF";

        [Required]
        [MaxLength(7)]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Formato hexadecimal inválido")]
        public string TextColor { get; set; } = "#000000";

        [Required]
        [Display(Name = "Última modificación")]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        public bool IsTerminalState { get; set; }

        public bool IsActionRequiredByUser { get; set; }

        public virtual ICollection<procedure_flow> ProcedureFlow { get; set; } = new List<procedure_flow>();

    }
}