using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("grades_group")]
    public class GradeGroup
    {
        [Key]
        [Column("GroupId")]
        public int Id { get; set; }

        [Column("GroupName")]
        public string GroupName { get; set; } = string.Empty;

        [Column("GradeLevelId")]
        public int GradeLevelId { get; set; }
    }
}
