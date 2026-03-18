namespace SchoolManager.Models
{
    public class users_auditlog
    {
        public int AuditId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = null!;
        public string TableName { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public users_user User { get; set; } = null!;
    }
}
