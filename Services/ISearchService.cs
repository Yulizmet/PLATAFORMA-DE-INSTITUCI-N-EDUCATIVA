namespace SchoolManager.Services;

public interface ISearchService
{
    Task<List<SearchResult>> SearchAsync(string term, string type);
}

public class SearchResult
{
    public int Id {get; set;}
    public int? PersonId {get; set;}
    public int? RoleId {get; set;}
    public int? UserId { get; set; }
    
    //Person
    public string? FirstName { get; set; }
    public string? LastNamePaternal { get; set; }
    public string? LastNameMaternal { get; set; }
    public string? FullName {get; set;}
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? Curp { get; set; }
    public string? Phone { get; set; }
    
    //User
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool? IsLocked { get; set; }
    public string? LockReason { get; set; }
    public DateTime? LastLoginDate { get; set; }
    
    //Role
    public string? RoleName { get; set; }
    public string? RoleDescription { get; set; }
    
    //General
    public bool? IsActive { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? Type {get; set;}
}