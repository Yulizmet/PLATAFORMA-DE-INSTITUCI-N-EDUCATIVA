using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class preenrollment_generations
    {
        [Key]
        public int IdGeneration { get; set; }

        [Required]
        public int Year { get; set; }

        public int? MaxFolios { get; set; }  // límite de folios para esta generación

        public ICollection<preenrollment_general> Students { get; set; } = new List<preenrollment_general>();
    }
}