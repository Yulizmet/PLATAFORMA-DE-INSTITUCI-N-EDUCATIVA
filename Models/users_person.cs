using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;

namespace SchoolManager.Models
{
    public class users_person
    {
        public int PersonId { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastNamePaternal { get; set; } = null!;
        public string LastNameMaternal { get; set; } = null!;

        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; } = null!;
        public string Curp { get; set; } = null!;

        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        [DefaultValue(true)]

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        [ValidateNever]
        public users_user User { get; set; } = null!;
    }
}
