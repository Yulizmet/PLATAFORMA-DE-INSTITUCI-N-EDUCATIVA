using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class preenrollment_addresses
    {
        [Key]
        public int IdAddress { get; set; }

        [Required]
        public int id_data { get; set; }

        [ForeignKey("id_data")]
        public preenrollment_general preenrollment_general { get; set; }

        [Required]
        public string street { get; set; }

        [Required]
        public string exterior_number { get; set; }

        [Required]
        public string interior_number { get; set; }

        [Required]
        public string postal_code { get; set; }

        [Required]
        public string neighborhood { get; set; }

        [Required]
        public string state { get; set; }

        [Required]
        public string city { get; set; }

        [Required]
        public string phone { get; set; }
    }
}