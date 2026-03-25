using SchoolManager.Models;

namespace SchoolManager.Areas.SocialService.ViewModels
{
    public class AsignarHorasViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? SemesterName { get; set; }
        public string? GroupName { get; set; }
        public int TotalBitacoras { get; set; }
        public int BitacorasPendientesCount { get; set; }
        public int TotalHorasPracticas { get; set; }
        public int TotalHorasServicioSocial { get; set; }
        public List<BitacoraPendiente> BitacorasPendientes { get; set; } = new();
        public int TotalHorasPracticasPendientes { get; set; }
        public int TotalHorasServicioSocialPendientes { get; set; }
    }

    public class BitacoraPendiente
    {
        public int LogId { get; set; }
        public string Week { get; set; } = string.Empty;
        public int HoursPracticas { get; set; }
        public int HoursServicioSocial { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
