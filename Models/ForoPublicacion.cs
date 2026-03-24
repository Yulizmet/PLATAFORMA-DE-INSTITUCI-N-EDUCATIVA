using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class ForoPublicacion
    {
        [Key]
        public int PublicacionId { get; set; }

        [Required]
        [MaxLength(250)]
        public string Titulo { get; set; } = null!;

        [Required]
        public string Descripcion { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string EstiloVisual { get; set; } = null!;

        [MaxLength(500)]
        public string? LinkExterno { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public users_user? Usuario { get; set; }

        public ICollection<ForoImagen> Imagenes { get; set; } = new List<ForoImagen>();
    }
}