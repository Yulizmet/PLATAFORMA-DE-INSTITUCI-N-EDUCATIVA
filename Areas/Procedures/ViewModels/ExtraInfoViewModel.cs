namespace SchoolManager.Areas.Procedures.ViewModels
{
    public class ExtraInfoViewModel
    {
        public int RequestId { get; set; }
        public string Folio { get; set; } = null!;
        public string ProcedureName { get; set; } = null!;
        public string CurrentStatus { get; set; } = null!;
        public string UserMessage { get; set; } = null!;
        public DateTime LastUpdate { get; set; }

        public string StudentFullName { get; set; } = null!;
        public string Matricula { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string ApplicantFolio { get; set; } = null!;
        public bool IsAspirante { get; set; }

        public List<DocumentDetail> Documents { get; set; } = new List<DocumentDetail>();
        public List<MonitoringStep> History { get; set; } = new List<MonitoringStep>();
    }

    public class DocumentDetail
    {
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
    }

    public class MonitoringStep
    {
        public string StatusName { get; set; } = null!;
        public string AdminComment { get; set; } = null!;
        public DateTime Date { get; set; }
        public string UpdatedBy { get; set; } = null!;
        public string BackgroundColor { get; set; } = "#6c757d";
        public string TextColor { get; set; } = "#ffffff";
    }
}