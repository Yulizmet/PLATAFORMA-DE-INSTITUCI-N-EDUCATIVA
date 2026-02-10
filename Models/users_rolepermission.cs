using System.Data;
using System.Security;

namespace SchoolManager.Models
{
    public class users_rolepermission
    {
        public int RolePermissionId { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public string Status { get; set; }

        public DateTime CreatedTime { get; set; }
        public users_role Role { get; set; }
        public users_permission Permission { get; set; }
    }
}
