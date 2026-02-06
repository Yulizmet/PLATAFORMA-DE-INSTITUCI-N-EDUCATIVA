using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class procedure_status
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(7)]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Formato hexadecimal inválido")]
        public string BackgroundColor { get; set; } = "#FFFFFF";

        [Required]
        [MaxLength(7)]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Formato hexadecimal inválido")]
        public string TextColor { get; set; } = "#000000";

        [Required]
        public int StepOrder { get; set; }

        [Required]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        public virtual ICollection<procedure_request> ProcedureRequests { get; set; } = new List<procedure_request>();
    }
}