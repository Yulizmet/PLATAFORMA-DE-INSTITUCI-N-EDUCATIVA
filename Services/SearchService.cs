using SchoolManager.Data;
using SchoolManager.Models;
using Microsoft.EntityFrameworkCore;

namespace SchoolManager.Services
{
    public class SearchService : ISearchService
    {
        private readonly AppDbContext _context;

        public SearchService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SearchResult>> SearchAsync(string term, string type)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<SearchResult>();

            return type.ToLower() switch
            {
                "persons" => await SearchPersons(term),
                "persons_no_account" => await SearchPersonsWithoutAccount(term),
                "students" => await SearchUsersByRole(term, "Student"),
                "teachers" => await SearchUsersByRole(term, "Teacher"),
                "admins" => await SearchUsersByRole(term, "Admin"),
                "users" => await SearchAllUsers(term),
                "roles" => await SearchRolesAsync(term),
                _ => new List<SearchResult>()
            };
        }

        private async Task<List<SearchResult>> SearchPersons(string term)
        {
            var query = await _context.Persons
                .Where(p => p.IsActive &&
                    (p.FirstName.Contains(term) ||
                     p.LastNamePaternal.Contains(term) ||
                     p.LastNameMaternal.Contains(term) ||
                     p.Curp.Contains(term) ||
                     p.Email.Contains(term)))
                .Take(10)
                .ToListAsync();

            return query.Select(p => new SearchResult
            {
                Id = p.PersonId,
                PersonId = p.PersonId,
                FirstName = p.FirstName,
                LastNamePaternal = p.LastNamePaternal,
                LastNameMaternal = p.LastNameMaternal,
                FullName = $"{p.FirstName} {p.LastNamePaternal} {p.LastNameMaternal}",
                BirthDate = p.BirthDate,
                Gender = p.Gender,
                Curp = p.Curp,
                Email = p.Email,
                Phone = p.Phone,
                IsActive = p.IsActive,
                CreatedDate = p.CreatedDate,
                Type = "person"
            }).ToList();
        }

        private async Task<List<SearchResult>> SearchUsersByRole(string term, string roleName)
        {
            var query = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.IsActive &&
                    u.UserRoles.Any(ur => ur.Role.Name == roleName && ur.IsActive) &&
                    (u.Person.FirstName.Contains(term) ||
                     u.Person.LastNamePaternal.Contains(term) ||
                     u.Person.LastNameMaternal.Contains(term) ||
                     u.Person.Curp.Contains(term) ||
                     u.Username.Contains(term) ||
                     u.Email.Contains(term)))
                .Take(10)
                .ToListAsync();

            return query.Select(u => new SearchResult
            {
                Id = u.UserId,
                PersonId = u.PersonId,
                UserId = u.UserId,
                FirstName = u.Person.FirstName,
                LastNamePaternal = u.Person.LastNamePaternal,
                LastNameMaternal = u.Person.LastNameMaternal,
                FullName = $"{u.Person.FirstName} {u.Person.LastNamePaternal} {u.Person.LastNameMaternal}",
                BirthDate = u.Person.BirthDate,
                Gender = u.Person.Gender,
                Curp = u.Person.Curp,
                Email = u.Email,
                Phone = u.Person.Phone,
                Username = u.Username,
                IsLocked = u.IsLocked,
                LockReason = u.LockReason,
                LastLoginDate = u.LastLoginDate,
                RoleName = roleName,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate,
                Type = roleName.ToLower()
            }).ToList();
        }

        private async Task<List<SearchResult>> SearchAllUsers(string term)
        {
            var query = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.IsActive &&
                    (u.Person.FirstName.Contains(term) ||
                     u.Person.LastNamePaternal.Contains(term) ||
                     u.Person.LastNameMaternal.Contains(term) ||
                     u.Username.Contains(term) ||
                     u.Email.Contains(term)))
                .Take(10)
                .ToListAsync();

            return query.Select(u => new SearchResult
            {
                Id = u.UserId,
                PersonId = u.PersonId,
                UserId = u.UserId,
                FirstName = u.Person.FirstName,
                LastNamePaternal = u.Person.LastNamePaternal,
                LastNameMaternal = u.Person.LastNameMaternal,
                FullName = $"{u.Person.FirstName} {u.Person.LastNamePaternal} {u.Person.LastNameMaternal}",
                BirthDate = u.Person.BirthDate,
                Gender = u.Person.Gender,
                Curp = u.Person.Curp,
                Email = u.Email,
                Phone = u.Person.Phone,
                Username = u.Username,
                IsLocked = u.IsLocked,
                LockReason = u.LockReason,
                LastLoginDate = u.LastLoginDate,
                RoleName = u.UserRoles.FirstOrDefault(ur => ur.IsActive)?.Role?.Name,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate,
                Type = "user"
            }).ToList();
        }
        
        private async Task<List<SearchResult>> SearchPersonsWithoutAccount(string term)
        {
            var query = await _context.Persons
                .Where(p => p.IsActive &&
                            p.User == null &&
                            (p.FirstName.Contains(term) ||
                             p.LastNamePaternal.Contains(term) ||
                             p.LastNameMaternal.Contains(term) ||
                             p.Curp.Contains(term) ||
                             p.Email.Contains(term)))
                .Take(10)
                .ToListAsync();

            return query.Select(p => new SearchResult
            {
                Id = p.PersonId,
                PersonId = p.PersonId,
                FirstName = p.FirstName,
                LastNamePaternal = p.LastNamePaternal,
                LastNameMaternal = p.LastNameMaternal,
                FullName = $"{p.FirstName} {p.LastNamePaternal} {p.LastNameMaternal}",
                BirthDate = p.BirthDate,
                Gender = p.Gender,
                Curp = p.Curp,
                Email = p.Email,
                Phone = p.Phone,
                IsActive = p.IsActive,
                CreatedDate = p.CreatedDate,
                Type = "person"
            }).ToList();
        }

        private async Task<List<SearchResult>> SearchRolesAsync(string term)
        {
            var query = await _context.Roles
                .Where(r => r.IsActive &&
                    (r.Name.Contains(term) || r.Description.Contains(term)))
                .Take(10)
                .ToListAsync();

            return query.Select(r => new SearchResult
            {
                Id = r.RoleId,
                RoleId = r.RoleId,
                RoleName = r.Name,
                RoleDescription = r.Description,
                IsActive = r.IsActive,
                CreatedDate = r.CreatedDate,
                Type = "role"
            }).ToList();
        }
    }
}