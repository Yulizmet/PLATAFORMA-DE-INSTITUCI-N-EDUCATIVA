using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    public class TutorshipAttendance
    {
        [Key]
        public int AttendanceId { get; set; }
        public int StudentId { get; set; } 
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
        public string GroupName { get; set; }
    }
}