using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class preenrollment_careers
    {
        [Key]
        public int IdCareer { get; set; }

        [Required]
        public string name_career { get; set; }

        public ICollection<preenrollment_general> preenrollment_general { get; set; }
    }
}