using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("social_service_assignment")]
    public class social_service_assignment
    {
        [Key]
        [Column("AssignmentId")]
        public int AssignmentId { get; set; }

        [Required]
        [Column("TeacherId")]
        public int TeacherId { get; set; }

        [Required]
        [Column("StudentId")]
        public int StudentId { get; set; }

        [MaxLength(100)]
        [Column("GroupName")]
        public string? GroupName { get; set; }

        [MaxLength(100)]
        [NotMapped]
        public string? SemesterName { get; set; }

        [MaxLength(150)]
        [NotMapped]
        public string? CareerName { get; set; }

        [Required]
        [Column("AssignedDate")]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [Required]
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [ForeignKey("TeacherId")]
        public virtual users_user? Teacher { get; set; }

        [ForeignKey("StudentId")]
        public virtual users_user? Student { get; set; }
    }
}
