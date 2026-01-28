using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class EntidadTest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
