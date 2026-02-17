using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class tutorship_attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsPresent { get; set; }

        [Required]
        public string GroupName { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual users_user Student { get; set; } = null!;

        [ForeignKey("TeacherId")]
        public virtual users_user Teacher { get; set; } = null!;
    }
}