using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SchoolManager.Models
{
    [Table("users_person", Schema = "dbo")]
    public class medical_person
    {
        [Key]
        [Column("PersonId")]
        public int Id { get; set; }

        [Column("FirstName")]
        public string? Nombre { get; set; }

        [Column("LastNamePaternal")]
        public string? ApellidoPaterno { get; set; }

        [Column("LastNameMaternal")]
        public string? ApellidoMaterno { get; set; }

        [Column("BirthDate")]
        public DateTime? FechaNacimiento { get; set; }
    }
}