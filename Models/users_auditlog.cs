namespace SchoolManager.Models
{
    public class users_auditlog
    {
        public int AuditId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public DateTime CreatedDate { get; set; }
        public users_user User { get; set; }
    }
}
