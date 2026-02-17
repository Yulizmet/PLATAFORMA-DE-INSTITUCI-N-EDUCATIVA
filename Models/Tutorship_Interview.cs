using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class TutorshipInterview
    {
        [Key]
        public int InterviewId { get; set; }
        public int StudentId { get; set; } 
        public DateTime DateCompleted { get; set; }
        public string Status { get; set; } 
    }
}