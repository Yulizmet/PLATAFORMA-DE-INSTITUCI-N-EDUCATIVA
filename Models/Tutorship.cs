using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class tutorship
    {
        [Key]
        public int TutorshipId { get; set; }
        public DateTime Date { get; set; }

        [Required]
        public string Topic { get; set; } = null!;

        [Required]
        public int TeacherId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual users_user Teacher { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual users_user Student { get; set; } = null!;
    }
}