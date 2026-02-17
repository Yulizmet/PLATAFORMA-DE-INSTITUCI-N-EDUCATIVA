using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class preenrollment_docs
    {
        [Key]
        public int IdDocument { get; set; }

        [Required]
        public int IdData { get; set; }

        [ForeignKey("IdData")]
        public virtual preenrollment_general General { get; set; } = null!;

        [Required]
        [Column(TypeName = "bit")]
        public bool Fotos { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool PagoExamen { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool ActaNacimiento { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool Curp { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool Certificados { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool ComprobanteDomicilio { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool CartaBuenaConducta { get; set; }
    }
}
