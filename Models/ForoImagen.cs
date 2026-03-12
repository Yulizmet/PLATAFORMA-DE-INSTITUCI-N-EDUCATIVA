using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class ForoImagen
    {
        [Key]
        public int ImagenId { get; set; }

        public int PublicacionId { get; set; }

        [MaxLength(500)]
        public string UrlImagen { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("PublicacionId")]
        public ForoPublicacion Publicacion { get; set; }
    }
}