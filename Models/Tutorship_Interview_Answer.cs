using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class TutorshipInterviewAnswer
    {
        [Key]
        public int AnswerId { get; set; }
        public int InterviewId { get; set; } 

        public string QuestionCategory { get; set; } 
        public string QuestionText { get; set; } 
        public string AnswerText { get; set; } 
    }
}