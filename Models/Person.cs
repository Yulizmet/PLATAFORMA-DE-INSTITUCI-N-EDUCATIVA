using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("users_person")]
    public class Person
    {
        [Key]
        [Column("PersonId")]
        public int Id { get; set; }

        [Column("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [Column("LastNamePaternal")]
        public string LastNamePaternal { get; set; } = string.Empty;

        [Column("LastNameMaternal")]
        public string LastNameMaternal { get; set; } = string.Empty;

        [Column("Gender")]
        public string Gender { get; set; } = string.Empty;

        [Column("BirthDate")]
        public DateTime BirthDate { get; set; }

        [Column("PhoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
