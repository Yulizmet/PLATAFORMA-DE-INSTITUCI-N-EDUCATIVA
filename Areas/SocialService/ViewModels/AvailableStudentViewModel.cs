namespace SchoolManager.Areas.SocialService.ViewModels
{
    /// <summary>
    /// ViewModel para mostrar estudiantes disponibles para asignar
    /// </summary>
    public class AvailableStudentViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SemesterName { get; set; }
        public string? GroupName { get; set; }
    }
}
