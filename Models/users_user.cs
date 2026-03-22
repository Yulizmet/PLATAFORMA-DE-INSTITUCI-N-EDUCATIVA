using System;
using System.ComponentModel;
using SchoolManager.Models;

namespace SchoolManager.Models
{
    public class users_user
    {
        public int UserId { get; set; }
        public int PersonId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsLocked { get; set; }
        public string LockReason { get; set; } = null!;
        public DateTime? LastLoginDate { get; set; }
        [DefaultValue(true)]

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public ICollection<ForoPublicacion> ForoPublicaciones { get; set; } = null!;
        public users_person Person { get; set; } = null!;

        public ICollection<procedure_request> ProcedureRequests { get; set; } = new List<procedure_request>();

        public ICollection<preenrollment_general> Preenrollments { get; set; } = new List<preenrollment_general>();

        public ICollection<users_userrole> UserRoles { get; set; } = null!;
        public ICollection<users_session> Sessions { get; set; } = null!;
        public ICollection<users_auditlog> AuditLogs { get; set; } = null!;
    }
}
