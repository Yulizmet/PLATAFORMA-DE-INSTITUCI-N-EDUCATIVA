namespace SchoolManager.Areas.Procedures.ViewModels
{
    public class ExtraInfoViewModel
    {
        public int RequestId { get; set; }
        public string Folio { get; set; }
        public string ProcedureName { get; set; }
        public string CurrentStatus { get; set; }
        public string UserMessage { get; set; }
        public DateTime LastUpdate { get; set; }

        public string StudentFullName { get; set; }
        public string Matricula { get; set; }
        public string Email { get; set; }

        public List<DocumentDetail> Documents { get; set; } = new List<DocumentDetail>();
        public List<MonitoringStep> History { get; set; } = new List<MonitoringStep>();
    }

    public class DocumentDetail
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    public class MonitoringStep
    {
        public string StatusName { get; set; }
        public string AdminComment { get; set; }
        public DateTime Date { get; set; }
        public string UpdatedBy { get; set; }
    }
}