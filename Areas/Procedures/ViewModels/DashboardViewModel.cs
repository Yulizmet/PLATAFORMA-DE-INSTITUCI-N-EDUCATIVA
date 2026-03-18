using Microsoft.EntityFrameworkCore;
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

        public int SelectedYear { get; set; }

        public int[] MonthlyRequests { get; set; } = new int[12];

        public double AvgResolutionDays { get; set; }

        public string AvgResolutionTime { get; set; } = "0d 0h 0m";
        public string AvgWaitTime { get; set; } = "0d 0h 0m";
        public double AvgResolutionHours { get; set; }
        public double AvgWaitHours { get; set; }

        public Dictionary<string, int> RequestsByArea { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> RequestsByType { get; set; } = new Dictionary<string, int>();
    }
}
