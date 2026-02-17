using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class tutorship_interview
    {
        [Key]
        public int InterviewId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public DateTime DateCompleted { get; set; }

        [Required]
        public string Status { get; set; } = null!;

        [Required]
        public string FilePath { get; set; } = null!;

        public virtual ICollection<tutorship_interview_answer> Answers { get; set; }

        [ForeignKey("StudentId")]
        public virtual users_user Student { get; set; } = null!;
    }
}