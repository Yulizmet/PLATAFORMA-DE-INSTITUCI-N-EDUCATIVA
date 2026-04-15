using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SchoolManager.Models
{
    public class preenrollment_infos
    {
        [Key]
        public int IdInfo { get; set; }

        [Required]
        public int id_data { get; set; }

        [ForeignKey("id_data")]
        [ValidateNever]
        public virtual preenrollment_general General { get; set; } = null!;

        [Required]
        public string beca { get; set; } = "No";  // Si tiene beca (Sí/No)

        public string? tipoBeca { get; set; }  // Detalles del tipo de beca (solo si "Sí")

        public bool comu_indi { get; set; }  // ¿Proviene de comunidad indígena? (Sí/No)

        public string? nombreComunidad { get; set; }  // Detalles de la comunidad indígena (solo si "Sí")

        public bool lengu_indi { get; set; }  // ¿Habla lengua indígena? (Sí/No)

        public string? nombreLenguaIndi { get; set; }  // Nombre de la lengua indígena (solo si "Sí")

        public bool incapa { get; set; }  // ¿Padece alguna discapacidad? (Sí/No)

        public string? tipoDiscapacidad { get; set; }  // Tipo de discapacidad (solo si "Sí")

        public bool disease { get; set; }  // ¿Padece alguna enfermedad? (Sí/No)

        public string? tipoEnfermedad { get; set; }  // Tipo de enfermedad (solo si "Sí")

        public string? comment { get; set; }  // ¿Cómo conoció la institución?
    }
}