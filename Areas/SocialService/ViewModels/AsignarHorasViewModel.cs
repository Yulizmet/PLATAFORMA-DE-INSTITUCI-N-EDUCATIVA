using SchoolManager.Models;

namespace SchoolManager.Areas.SocialService.ViewModels
{
    public class AsignarHorasViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? GroupName { get; set; }
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
