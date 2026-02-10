using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class preenrollment_tutors
    {
        [Key]
        public int IdTutor { get; set; }


        public int id_data { get; set; }
        public preenrollment_general preenrollment_general { get; set; }

        public string relationship { get; set; }
        public string paternal_last_name { get; set; }
        public string maternal_last_name { get; set; }
        public string name { get; set; }
        public string home_phone { get; set; }
        public string work_phone { get; set; }
    }

}
