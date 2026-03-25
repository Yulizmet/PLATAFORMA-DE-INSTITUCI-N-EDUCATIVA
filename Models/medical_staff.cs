using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SchoolManager.Models
{
    [Table("medical_staff")]
    public class medical_staff
    {
        [Key]
        [Column("staff_id")]
        public int Id { get; set; }

        [Column("PersonId")]
        public int PersonId { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("shift")]
        public string? Shift { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        [ValidateNever]
        public string? Curp { get; set; }

        [NotMapped]
        [ValidateNever]
        public string? NombreCompleto { get; set; }

        [NotMapped]
        [ValidateNever]
        public string? Correo { get; set; }
    }
}