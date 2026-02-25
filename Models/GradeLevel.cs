using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("grades_grade_level")]
    public class GradeLevel
    {
        [Key]
        [Column("GradeLevelId")]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; } = string.Empty;
    }
}
