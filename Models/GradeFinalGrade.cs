using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("grades_final_grades")]
    public class GradeFinalGrade
    {
        [Key]
        [Column("FinalGradeId")]
        public int Id { get; set; }

        [Column("StudentId")]
        public int StudentId { get; set; }

        [Column("SubjectId")]
        public int SubjectId { get; set; }

        [Column("GroupId")]
        public int GroupId { get; set; }

        [Column("Value")]
        public double Value { get; set; }

        [Column("Passed")]
        public bool Passed { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }
    }
}
