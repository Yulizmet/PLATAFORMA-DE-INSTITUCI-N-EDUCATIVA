using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SchoolManager.Models
{
    public class preenrollment_general
    {
        [Key]
        public int IdData { get; set; }

        [Required]
        public int IdCareer { get; set; }

        [ForeignKey("IdCareer")]
        [ValidateNever]
        public preenrollment_careers Career { get; set; } = null!;

        [Required]
        public int IdGeneration { get; set; }

        [ForeignKey("IdGeneration")]
        [ValidateNever]
        public preenrollment_generations Generation { get; set; } = null!;

        public int? PersonId { get; set; }

        [ForeignKey("PersonId")]
        [ValidateNever]
        public users_person? Person { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public users_user? User { get; set; }

        public int? ProcedureRequestId { get; set; }

        [ForeignKey("ProcedureRequestId")]
        [ValidateNever]
        public virtual procedure_request? ProcedureRequest { get; set; }

        [MaxLength(10)]
        public string? BloodType { get; set; }

        public DateTime? CreateStat { get; set; }

        [MaxLength(50)]
        public string? Folio { get; set; }

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

        [MaxLength(30)]
        public string? Matricula { get; set; }

        [ValidateNever]
        public ICollection<preenrollment_schools> Schools { get; set; } = new List<preenrollment_schools>();

        [ValidateNever]
        public ICollection<preenrollment_addresses> Addresses { get; set; } = new List<preenrollment_addresses>();

        [ValidateNever]
        public ICollection<preenrollment_infos> Infos { get; set; } = new List<preenrollment_infos>();

        [ValidateNever]
        public ICollection<preenrollment_tutors> Tutors { get; set; } = new List<preenrollment_tutors>();


    }
}