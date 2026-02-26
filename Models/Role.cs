using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("users_role")]
    public class Role
    {
        [Key]
        [Column("RoleId")]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Column("Description")]
        public string Description { get; set; } = string.Empty;
    }
}
