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

        [Required]
        public int IdGeneration { get; set; }

        public preenrollment_generations Generation { get; set; } = null!;


        [Required, MaxLength(100)]
        public string PaternalLastName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string MaternalLastName { get; set; } = null!;

        [Required, MaxLength(20)]
        public string Gender { get; set; } = null!;

        [Required]
        public DateTime BirthDate { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(18)]
        public string Curp { get; set; } = null!;

        [MaxLength(10)]
        public string? BloodType { get; set; }

        public DateTime? CreateStat { get; set; }

        [Required, MaxLength(50)]
        public string Folio { get; set; } = null!;

        [Required, MaxLength(30)]
        public string MaritalStatus { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Nationality { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Occupation { get; set; }

        [Required]
        public bool Work { get; set; }

        [MaxLength(200)]
        public string? WorkAddress { get; set; }

        [MaxLength(20)]
        public string? WorkPhone { get; set; }

        [Required, MaxLength(10)]
        public string Matricula { get; set; } = null!;


        public preenrollment_careers Career { get; set; } = null!;

        public ICollection<preenrollment_schools> Schools { get; set; } = new List<preenrollment_schools>();
        public ICollection<preenrollment_addresses> Addresses { get; set; } = new List<preenrollment_addresses>();
    }
}