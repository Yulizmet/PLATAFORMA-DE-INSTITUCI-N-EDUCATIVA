using System;

namespace SchoolManager.ViewModels
{
    public class TutorshipStatisticsVM
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string InterviewStatus { get; set; } = string.Empty;
        public string PerformanceLevel { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int TotalInterviews { get; set; }
        public int TotalMonitorings { get; set; }
        public DateTime? LastDate { get; set; }
    }
}
