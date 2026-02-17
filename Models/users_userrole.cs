using System.Data;

namespace SchoolManager.Models
{
    public class users_userrole
    {
        public int UserRoleId { get; set; }

        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public users_user User { get; set; }
        public users_role Role { get; set; }
    }
}
