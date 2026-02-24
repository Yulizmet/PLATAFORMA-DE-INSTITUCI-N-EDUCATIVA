namespace SchoolManager.Areas.Procedures.ViewModels
{
    public class StudentExpedienteViewModel
    {
        public int PersonId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Matricula { get; set; }
        public string? Folio { get; set; }
        public string Email { get; set; } = null!;
        public string? Username { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CareerName { get; set; }
    }
}