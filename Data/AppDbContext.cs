using Microsoft.EntityFrameworkCore;
using SchoolManager.Models;

namespace SchoolManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        //Add Entity Sample

        //public DbSet<EntidadTest> EntidadTests { get; set; }
        //public DbSet<EntidadTest2cs> EntidadTest2cs { get; set; }
    }
}
