using System;

namespace SchoolManager.Models
{
    public class users_user
    {
        public int UserId { get; set; }
        public int PersonId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsLocked { get; set; }
        public string LockReason { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public users_person Person { get; set; }
        public ICollection<users_userrole> UserRoles { get; set; }
        public ICollection<users_session> Sessions { get; set; }
        public ICollection<users_auditlog> AuditLogs { get; set; }
    }
}
