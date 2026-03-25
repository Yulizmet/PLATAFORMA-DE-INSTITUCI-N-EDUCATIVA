namespace SchoolManager.Areas.Procedures.ViewModels
{
    public class StaffViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PersonId { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastNamePaternal { get; set; } = null!;
        public string LastNameMaternal { get; set; } = null!;
        public string Curp { get; set; } = null!;
        public string BirthDate { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string Nationality { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? NewPassword { get; set; }

        public int IdJobPosition { get; set; }
        public string JobPositionName { get; set; } = null!;

        public string AreaName { get; set; } = null!;
        public int IdArea { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsActive { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
        public string RolesDisplay => string.Join(", ", Roles);

        public string Initials => !string.IsNullOrEmpty(FullName)
            ? string.Join("", FullName.Split(' ').Where(x => x.Length > 0).Select(x => x[0]).Take(2)).ToUpper()
            : "??";
    }
}