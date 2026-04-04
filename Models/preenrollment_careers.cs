using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class preenrollment_careers
    {
        [Key]
        public int IdCareer { get; set; }

        public string name_career { get; set; }

        public bool IsActive { get; set; } = true; // 👈 nuevo campo

        public ICollection<preenrollment_general> preenrollment_general { get; set; }
    }
}