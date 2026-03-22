using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable disable


namespace SchoolManager.Models
{
    [Table("preenrollment_general", Schema = "dbo")]
    public class medical_preenrollmentgeneral
    {
        [Key]
        [Column("IdData")]
        public int IdData { get; set; }

        [Column("Matricula")]
        public string Matricula { get; set; }

        [Column("BloodType")]
        public string BloodType { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }
    }
}