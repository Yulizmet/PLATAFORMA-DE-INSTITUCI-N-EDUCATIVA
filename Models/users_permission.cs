using System.ComponentModel;
namespace SchoolManager.Models

{
    public class users_permission
    {
        public int PermissionId { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        [DefaultValue(true)]

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<users_rolepermission> RolePermissions { get; set; }
    }
}
