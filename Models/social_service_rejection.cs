using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("social_service_rejection")]
    public class social_service_rejection
    {
        [Key]
        [Column("RejectionId")]
        public int RejectionId { get; set; }

        [Required]
        [Column("StudentId")]
        public int StudentId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Week")]
        public string Week { get; set; } = null!;

        [Column("RejectionReason")]
        public string? RejectionReason { get; set; }

        [Column("RejectedBy")]
        public int RejectedBy { get; set; }

        [Column("RejectedAt")]
        public DateTime RejectedAt { get; set; } = DateTime.Now;

        [Column("IsRead")]
        public bool IsRead { get; set; } = false;

        [ForeignKey("StudentId")]
        public virtual users_user? Student { get; set; }

        [ForeignKey("RejectedBy")]
        public virtual users_user? Teacher { get; set; }
    }
}
