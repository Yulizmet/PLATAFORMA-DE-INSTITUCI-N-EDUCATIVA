// Areas/Grades/ViewModels/Enrollment/EnrollmentViewModel.cs
namespace SchoolManager.Areas.Grades.ViewModels.Enrollment
{
    public class EnrollmentViewModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string Matricula { get; set; } = null!;
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public string GradeLevelName { get; set; } = null!;
    }

    public class EnrollmentCreateViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public string GradeLevelName { get; set; } = null!;
        public List<StudentSelectionViewModel> AvailableStudents { get; set; } = new();
    }

    public class StudentSelectionViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = null!; 
        public string Matricula { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsSelected { get; set; }
        public string? CurrentGroupName { get; set; }
        public string? CurrentGradeLevelName { get; set; }
    }
}