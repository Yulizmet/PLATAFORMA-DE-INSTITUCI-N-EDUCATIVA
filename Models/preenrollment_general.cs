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
        public preenrollment_careers Career { get; set; } = null!;

        [Required]
        public int IdGeneration { get; set; }

        // Relación con users_person
        public int? PersonId { get; set; }

        [ForeignKey("PersonId")]
        public users_person? Person { get; set; }

        // Relación con usuario del sistema (cuando se cree la cuenta)
        public int? UserId { get; set; }

        public users_user? User { get; set; }

        public int? ProcedureRequestId { get; set; }

        [ForeignKey("ProcedureRequestId")]
        public virtual procedure_request? ProcedureRequest { get; set; }

        public preenrollment_generations Generation { get; set; } = null!;

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

        // Matrícula se genera después en otro proceso
        [MaxLength(10)]
        public string? Matricula { get; set; }

        public ICollection<preenrollment_schools> Schools { get; set; } = new List<preenrollment_schools>();
        public ICollection<preenrollment_addresses> Addresses { get; set; } = new List<preenrollment_addresses>();
        public ICollection<preenrollment_infos> Infos { get; set; } = new List<preenrollment_infos>();
        public ICollection<preenrollment_tutors> Tutors { get; set; } = new List<preenrollment_tutors>();
    }
}