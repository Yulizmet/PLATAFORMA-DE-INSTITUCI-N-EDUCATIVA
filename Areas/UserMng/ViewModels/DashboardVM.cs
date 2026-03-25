namespace SchoolManager.Areas.UserMng.ViewModels;

public class DashboardVM
{
    public int TotalActiveUsers { get; set; }
    public int TotalActivePersons { get; set; }
    public List<RoleCardVM> Roles { get; set; } = new();
}

public class RoleCardVM
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public int ActiveCount { get; set; }
    public string Icon { get; set; }
    public string Color { get; set; }
    public string Url { get; set; }
}