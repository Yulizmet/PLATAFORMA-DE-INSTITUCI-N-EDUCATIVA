using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("medical_permissions")]
    public class medical_permissions
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("staff_id")]
        public int StaffId { get; set; }

        [Column("ver")]
        public bool Ver { get; set; }

        [Column("modificar")]
        public bool Modificar { get; set; }

        [Column("agregar")]
        public bool Agregar { get; set; }

        [Column("borrar")]
        public bool Borrar { get; set; }
    }
}