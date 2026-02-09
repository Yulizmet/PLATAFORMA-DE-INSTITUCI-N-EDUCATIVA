using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class preenrollment_addresses
    {
        [Key]
        public int IdAddress { get; set; }


        public int id_data { get; set; }
        public preenrollment_general preenrollment_general { get; set; }

        public string street { get; set; }
        public string exterior_number { get; set; }
        public string interior_number { get; set; }
        public string postal_code { get; set; }
        public string neighborhood { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string phone { get; set; }
    }

}
