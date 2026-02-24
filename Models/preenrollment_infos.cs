using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class preenrollment_infos
    {
        [Key]
        public int IdInfo { get; set; }

        [Required]
        public int id_data { get; set; }

        [ForeignKey("id_data")]
        public preenrollment_general preenrollment_general { get; set; }

        [Required]
        public string beca { get; set; }

        [Required]
        public bool comu_indi { get; set; }

        [Required]
        public bool lengu_indi { get; set; }

        [Required]
        public bool incapa { get; set; }

        [Required]
        public bool disease { get; set; }

        [Required]
        public string comment { get; set; }
    }
}