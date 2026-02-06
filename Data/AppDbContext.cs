using Microsoft.EntityFrameworkCore;
using SchoolManager.Models;

namespace SchoolManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        //Add Entity Sample

        public DbSet<procedure_areas> ProcedureAreas { get; set; }

        public DbSet<procedure_documents> ProcedureDocuments { get; set; }

        public DbSet<procedure_monitoring> ProcedureMonitoring { get; set; }

        public DbSet<procedure_request> ProcedureRequest { get; set; }

        public DbSet<procedure_status> ProcedureStatus { get; set; }

        public DbSet<procedure_type_documents> ProcedureTypeDocuments { get; set; }

        public DbSet<procedure_type_requirements> ProcedureTypeRequirements { get; set; }

        public DbSet<procedure_types> ProcedureTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<procedure_status>().ToTable("procedure_status");
            modelBuilder.Entity<procedure_areas>().ToTable("procedure_areas");
            modelBuilder.Entity<procedure_documents>().ToTable("procedure_documents");
            modelBuilder.Entity<procedure_request>().ToTable("procedure_request");
            modelBuilder.Entity<procedure_type_documents>().ToTable("procedure_type_documents");
            modelBuilder.Entity<procedure_type_requirements>().ToTable("procedure_type_requirements");
            modelBuilder.Entity<procedure_types>().ToTable("procedure_types");
            modelBuilder.Entity<procedure_monitoring>().ToTable("procedure_monitoring");

            modelBuilder.Entity<procedure_monitoring>()
                .HasOne(pm => pm.ProcedureRequest)
                .WithMany(pr => pr.ProcedureMonitorings)
                .HasForeignKey(pm => pm.IdProcedure)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<procedure_monitoring>()
                .HasOne(pm => pm.ProcedureStatus)
                .WithMany()
                .HasForeignKey(pm => pm.IdStatus)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
