using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class preenrollment_tutors
    {
        [Key]
        public int IdTutor { get; set; }

        [Required]
        public int id_data { get; set; }

        [ForeignKey("id_data")]
        public preenrollment_general preenrollment_general { get; set; }

        [Required]
        public string relationship { get; set; }

        [Required]
        public string paternal_last_name { get; set; }

        [Required]
        public string maternal_last_name { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public string home_phone { get; set; }

        [Required]
        public string work_phone { get; set; }
    }
}