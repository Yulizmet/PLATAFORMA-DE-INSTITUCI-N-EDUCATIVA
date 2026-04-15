using SchoolManager.Models;

namespace SchoolManager.Areas.SocialService.ViewModels
{
    public class CoordinatorTeacherViewModel
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int AlumnosAsignados { get; set; }
        public int BitacorasPendientes { get; set; }
        public int TotalBitacoras { get; set; }
    }

    public class CoordinatorStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? CareerName { get; set; }
        public string? SemesterName { get; set; }
        public string? GroupName { get; set; }
        public string? TeacherName { get; set; }
        public int TotalBitacoras { get; set; }
        public int BitacorasPendientes { get; set; }
        public int TotalHorasPracticas { get; set; }
        public int TotalHorasServicioSocial { get; set; }
    }

    public class CoordinatorAttendanceDetailViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public List<social_service_attendance> Attendances { get; set; } = new();
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
    }
}
