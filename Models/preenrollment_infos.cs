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
        [ForeignKey("IdData")]
        public virtual preenrollment_general General { get; set; } = null!;

        public string beca { get; set; }
        public bool comu_indi { get; set; }
        public bool lengu_indi { get; set; }
        public bool incapa { get; set; }
        public bool disease { get; set; }
        public string comment { get; set; }
    }

}
