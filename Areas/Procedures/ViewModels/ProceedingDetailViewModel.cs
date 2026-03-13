namespace SchoolManager.Areas.Procedures.ViewModels
{
    public class ProceedingDetailViewModel
    {
        // Identity

        public int PersonId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastNamePaternal { get; set; } = null!;
        public string LastNameMaternal { get; set; } = null!;
        public string Curp { get; set; } = null!;
        public string BirthDate { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string Nationality { get; set; } = null!;

        // Account
        public string Username { get; set; } = null!;
        public string UserStatus { get; set; } = null!;
        public string? LastLogin { get; set; }

        // School Information
        public string Matricula { get; set; } = null!;
        public string Folio { get; set; } = null!;
        public string CareerName { get; set; } = null!;
        public string Generation { get; set; } = null!;

        // General/Medical Information
        public string BloodType { get; set; } = null!;
        public string MaritalStatus { get; set; } = null!;
        public string Occupation { get; set; } = null!;
        public bool DoesWork { get; set; }
        public string? WorkPhone { get; set; }
        public string? WorkAddress { get; set; }
        public string Beca { get; set; } = "NO";
        public bool IsIndigena { get; set; }
        public bool HasIncapacidad { get; set; }
        public bool HasDisease { get; set; }
        public string? HealthComments { get; set; }

        // Scholar Information
        public string PreviousSchool { get; set; } = null!;
        public decimal PreviousAverage { get; set; }
        public string PreviousDegree { get; set; } = null!;

        // Contact and Detailed Address
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string ExtNum { get; set; } = null!;
        public string? IntNum { get; set; }
        public string Colony { get; set; } = null!;
        public string ZipCode { get; set; } = null!;
        public string CityState { get; set; } = null!;

        public List<EnrollmentDocumentDetail> DigitalFiles { get; set; } = new List<EnrollmentDocumentDetail>();

        public class EnrollmentDocumentDetail
        {
            public string FileName { get; set; } = null!;
            public string FilePath { get; set; } = null!;
        }
    }
}