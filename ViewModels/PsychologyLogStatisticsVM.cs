using System;

namespace SchoolManager.ViewModels
{
    public class PsychologyLogStatisticsVM
    {
        public int Id { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string EnrollmentOrMatricula { get; set; } = string.Empty;
        public string AttendanceStatus { get; set; } = string.Empty;
        public string Observations { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
