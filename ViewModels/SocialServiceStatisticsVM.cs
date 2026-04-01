namespace SchoolManager.ViewModels
{
    public class SocialServiceStatisticsVM
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public double HoursPracticas { get; set; }
        public double HoursServicioSocial { get; set; }
        public double TotalHours { get; set; }
        public double AttendanceRate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? LastUpdate { get; set; }
        public int TotalAttendances { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalJustified { get; set; }
        public DateTime? LastAttendanceDate { get; set; }
    }
}
