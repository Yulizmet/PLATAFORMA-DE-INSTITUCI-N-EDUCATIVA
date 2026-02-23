// Models/grades_enrollment.cs
namespace SchoolManager.Models
{
    public class grades_enrollment
    {
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; } // FK a users_user
        public int GroupId { get; set; }    // FK a grades_group

        // Navigation properties
        public users_user Student { get; set; } = null!;
        public grades_group Group { get; set; } = null!;
    }
}