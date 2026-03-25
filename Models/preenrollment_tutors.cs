using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SchoolManager.Models
{
    public class preenrollment_tutors
    {
        [Key]
        public int IdTutor { get; set; }

        [Required]
        public int id_data { get; set; }

        [ForeignKey("id_data")]
        [ValidateNever]
        public virtual preenrollment_general General { get; set; } = null!;

        public string? relationship { get; set; }
        public string? paternal_last_name { get; set; }
        public string? maternal_last_name { get; set; }
        public string? name { get; set; }
        public string? home_phone { get; set; }
        public string? work_phone { get; set; }
    }
}