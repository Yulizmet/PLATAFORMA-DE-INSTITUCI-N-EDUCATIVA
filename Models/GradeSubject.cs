using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("grades_subjects")]
    public class GradeSubject
    {
        [Key]
        [Column("SubjectId")]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Column("Code")]
        public string Code { get; set; } = string.Empty;

        [Column("Credits")]
        public int Credits { get; set; }
    }
}
