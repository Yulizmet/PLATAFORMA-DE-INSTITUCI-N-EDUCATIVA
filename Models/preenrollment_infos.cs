using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class preenrollment_infos
    {
        [Key]
        public int IdInfo { get; set; }


        public int id_data { get; set; }
        public preenrollment_general preenrollment_general { get; set; }

        public string beca { get; set; }
        public bool comu_indi { get; set; }
        public bool lengu_indi { get; set; }
        public bool incapa { get; set; }
        public bool disease { get; set; }
        public string comment { get; set; }
    }

}
