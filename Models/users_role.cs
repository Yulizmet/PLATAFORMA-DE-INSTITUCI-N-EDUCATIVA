namespace SchoolManager.Models
{
    public class users_role
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<users_userrole> UserRoles { get; set; }
        public ICollection<users_rolepermission> RolePermissions { get; set; }
    }
}
