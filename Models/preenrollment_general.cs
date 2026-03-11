using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models;

public class preenrollment_general
{
    [Key]
    public int IdData { get; set; }
    public int IdCareer { get; set; }
    public int IdGeneration { get; set; }
    public int? UserId { get; set; }
    public int? ProcedureRequestId { get; set; }
    public string? BloodType { get; set; }
    public DateTime? CreateStat { get; set; }
    public string Folio { get; set; } = null!;
    public string MaritalStatus { get; set; } = null!;
    public string Nationality { get; set; } = null!;
    public string? Occupation { get; set; }
    public bool Work { get; set; }
    public string? WorkAddress { get; set; }
    public string? WorkPhone { get; set; }
    public string Matricula { get; set; } = null!;

    public preenrollment_careers? Career { get; set; }
    public preenrollment_generations? Generation { get; set; }
    public users_user? User { get; set; }
    public procedure_request? ProcedureRequest { get; set; }
    public ICollection<preenrollment_addresses>? Addresses { get; set; }
    public ICollection<preenrollment_schools>? Schools { get; set; }
    public ICollection<preenrollment_infos>? Infos { get; set; }
    public ICollection<preenrollment_tutors>? Tutors { get; set; }
}