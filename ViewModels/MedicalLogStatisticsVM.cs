using System;

namespace SchoolManager.ViewModels
{
    public class MedicalLogStatisticsVM
    {
        public int Id { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string EnrollmentOrMatricula { get; set; } = string.Empty;
        public string ConsultationReason { get; set; } = string.Empty;
        public string VitalSigns { get; set; } = string.Empty;
        public string Observations { get; set; } = string.Empty;
        public string TreatmentAction { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
