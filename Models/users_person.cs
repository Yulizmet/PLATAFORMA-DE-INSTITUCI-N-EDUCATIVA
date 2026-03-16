using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SchoolManager.Models
{
    public class users_person
    {
        public int PersonId { get; set; }

        public string FirstName { get; set; }
        public string LastNamePaternal { get; set; }
        public string LastNameMaternal { get; set; }

        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public string Curp { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        [DefaultValue(true)]

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        [ValidateNever]
        public users_user? User { get; set; }
    }
}

