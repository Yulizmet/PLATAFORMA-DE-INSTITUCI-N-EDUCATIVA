using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class tutorship_interview_answer
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        public int InterviewId { get; set; }

        [Required]
        public string QuestionCategory { get; set; } = null!;

        [Required]
        public string QuestionText { get; set; } = null!;

        [Required]
        public string AnswerText { get; set; } = null!;

        [ForeignKey("InterviewId")]
        public virtual tutorship_interview Interview { get; set; } = null!;
    }
}