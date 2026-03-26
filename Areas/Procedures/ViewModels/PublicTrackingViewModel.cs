using System;

namespace SchoolManager.Areas.Procedures.ViewModels
{
    public class PublicTrackingViewModel
    {
        public string Folio { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; }
        public string AdminComment { get; set; } = string.Empty;
    }
}