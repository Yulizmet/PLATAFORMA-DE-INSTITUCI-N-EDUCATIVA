using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    [Table("users_userrole")]
    public class UserRole
    {
        [Key]
        [Column("UserRoleId")]
        public int Id { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("RoleId")]
        public int RoleId { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }
    }
}
