using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class preenrollment_schools
    {
        [Key]
        public int IdSchool { get; set; }

        [Required]
        public int id_data { get; set; }

        [ForeignKey("id_data")]
        public preenrollment_general preenrollment_general { get; set; } = null!;

        [Required]
        public string school { get; set; } = null!;

        [Required]
        public string degree { get; set; } = null!;

        [Required]
        public string state { get; set; } = null!;

        [Required]
        public string city { get; set; } = null!;

        [Required]
        public decimal average { get; set; }

        public DateTime? start_date { get; set; }  // Nullable como en BD

        public DateTime? end_date { get; set; }    // Nullable como en BD

        [Required]
        public string study_system { get; set; } = null!;

        [Required]
        public string high_school_type { get; set; } = null!;
    }
}