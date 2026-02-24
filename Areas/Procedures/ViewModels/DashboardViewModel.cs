using SchoolManager.Models;

namespace SchoolManager.Areas.Procedures.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalRequests { get; set; }
        public int ActionRequired { get; set; }
        public int InProgress { get; set; }
        public int Done { get; set; }
        public int Cancelled { get; set; }

        public List<procedure_request> Recientes { get; set; } = new();

        public List<int> MonthlyRequests { get; set; }
    }
}
