using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class preenrollment_general
    {
        [Key]
        public int IdData { get; set; }

        [Required]
        public int IdCareer { get; set; }

        [ForeignKey("IdCareer")]
        public virtual preenrollment_careers Career { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string PaternalLastName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string MaternalLastName { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Gender { get; set; } = null!;

        [Required]
        public DateTime? BirthDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string MaritalStatus { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Nationality { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(18)]
        public string Curp { get; set; } = null!;

        [Required]
        [Column(TypeName = "bit")]
        public bool Work { get; set; }

        [MaxLength(100)]
        public string? Occupation { get; set; }

        [MaxLength(200)]
        public string? WorkAddress { get; set; }

        [MaxLength(20)]
        public string? WorkPhone { get; set; }

        [Required]
        [MaxLength(50)]
        public string Folio { get; set; } = null!;

        public DateTime? CreateStat { get; set; }

        [MaxLength(10)]
        public string? BloodType { get; set; }

        public virtual ICollection<preenrollment_schools> Schools { get; set; }
            = new List<preenrollment_schools>();

        public virtual ICollection<preenrollment_addresses> Addresses { get; set; }
            = new List<preenrollment_addresses>();
    }
}