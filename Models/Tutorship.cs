using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class Tutorship
    {
        [Key]
        public int TutorshipId { get; set; }
        public DateTime Date { get; set; }
        public string Topic { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }
    }
}