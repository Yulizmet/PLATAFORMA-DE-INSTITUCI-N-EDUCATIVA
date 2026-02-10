namespace SchoolManager.Models
{
    public class users_session
    {
        public int SessionId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        //public DateTime Expiration {  get; set;}
        //public string Ip {  get; set;}
        //public string UserAgent { get; set; }
        public string UserAgent { get; set; }
        public DateTime CreatedDate { get; set; }
        public users_user User { get; set; }
    }
}
