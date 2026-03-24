// Models/grades_enrollment.cs
namespace SchoolManager.Models
{
    public class grades_enrollment
    {
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; }
        public int GroupId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime EnrolledAt { get; set; } = DateTime.Now;

        public users_user Student { get; set; } = null!;
        public grades_group Group { get; set; } = null!;
    }
}