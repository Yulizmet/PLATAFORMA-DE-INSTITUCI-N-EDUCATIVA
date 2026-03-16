using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SchoolManager.Models
{
    public class preenrollment_schools
    {
        [Key]
        public int IdSchool { get; set; }

        [Required]
        public int id_data { get; set; }

        [ForeignKey("id_data")]
        [ValidateNever]
        public virtual preenrollment_general General { get; set; } = null!;

        public string? school { get; set; }
        public string? degree { get; set; }
        public string? state { get; set; }
        public string? city { get; set; }

        public decimal? average { get; set; }

        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }

        public string? study_system { get; set; }
        public string? high_school_type { get; set; }
    }
}