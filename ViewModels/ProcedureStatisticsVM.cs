using System;

namespace SchoolManager.ViewModels
{
    public class ProcedureStatisticsVM
    {
        public int Id { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string ProcedureType { get; set; } = string.Empty;
        public string AreaName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string InternalCode { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int DaysElapsed { get; set; }
    }
}
