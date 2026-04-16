using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class tutorship_suggested_topic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } // Ej: "Prevención de Adicciones"

        public string Description { get; set; } // Detalles y sugerencias para el profe

        [Required]
        public DateTime StartDate { get; set; } // Lunes de esa semana

        [Required]
        public DateTime EndDate { get; set; } // Viernes/Domingo de esa semana

        public string FilePath { get; set; } // Ruta del PDF/Documento de apoyo (puede ser null)

        public int CreatedBy { get; set; } // El ID del administrador que lo creó
        public DateTime CreatedAt { get; set; }
    }
}