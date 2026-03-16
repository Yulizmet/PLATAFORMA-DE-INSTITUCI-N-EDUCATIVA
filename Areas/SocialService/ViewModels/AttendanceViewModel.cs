using SchoolManager.Models;

namespace SchoolManager.Areas.SocialService.ViewModels
{
    public class AttendanceViewModel
    {
        public social_service_assignment Assignment { get; set; }
        public bool HasAttendanceToday { get; set; }
        public bool IsPresentToday { get; set; }
        public List<social_service_attendance> RecentAttendances { get; set; } = new();
    }

    public class AttendanceListViewModel
    {
        public List<AttendanceViewModel> Students { get; set; } = new();
        public DateTime Today { get; set; } = DateTime.Today;
    }
}
