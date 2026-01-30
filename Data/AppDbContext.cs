using Microsoft.EntityFrameworkCore;
using SchoolManager.Models;

namespace SchoolManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        //Add Entity Sample

        public DbSet<ProcedureAreas> ProcedureAreas { get; set; }

        public DbSet<ProcedureDocuments> ProcedureDocuments { get; set; }

        public DbSet<ProcedureMonitoring> ProcedureMonitoring { get; set; }

        public DbSet<ProcedureRequest> ProcedureRequest { get; set; }

        public DbSet<ProcedureStatus> ProcedureStatus { get; set; }

        public DbSet<ProcedureTypeDocuments> ProcedureTypeDocuments { get; set; }

        public DbSet<ProcedureTypeRequirements> ProcedureTypeRequirements { get; set; }

        public DbSet<ProcedureTypes> ProcedureTypes { get; set; }
    }
}
