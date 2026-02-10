using Microsoft.EntityFrameworkCore;
using SchoolManager.Models;

namespace SchoolManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
        public DbSet<SchoolManager.Models.preenrollment_general> preenrollment_general { get; set; } = default!;
        public DbSet<SchoolManager.Models.preenrollment_addresses> preenrollment_addresses { get; set; } = default!;
        public DbSet<SchoolManager.Models.preenrollment_careers> preenrollment_careers { get; set; } = default!;
        public DbSet<SchoolManager.Models.preenrollment_infos> preenrollment_infos { get; set; } = default!;
        public DbSet<SchoolManager.Models.preenrollment_schools> preenrollment_schools { get; set; } = default!;
        public DbSet<SchoolManager.Models.preenrollment_tutors> preenrollment_tutors { get; set; } = default!;
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

                 // Configurar la relación entre preenrollment_general y preenrollment_careers
            modelBuilder.Entity<preenrollment_general>()
                .HasOne(p => p.Career)
                .WithMany(c => c.preenrollment_general)
                .HasForeignKey(p => p.IdCareer)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar la relación uno-a-muchos entre preenrollment_general y preenrollment_addresses
            modelBuilder.Entity<preenrollment_addresses>()
                .HasOne(a => a.preenrollment_general)
                .WithMany(g => g.Addresses)
                .HasForeignKey(a => a.id_data)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar la relación uno-a-muchos entre preenrollment_general y preenrollment_schools
            modelBuilder.Entity<preenrollment_schools>()
                .HasOne(s => s.preenrollment_general)
                .WithMany(g => g.Schools)
                .HasForeignKey(s => s.id_data)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar la relación uno-a-uno entre preenrollment_general y preenrollment_infos
            modelBuilder.Entity<preenrollment_infos>()
                .HasOne(i => i.preenrollment_general)
                .WithMany() // No hay colección en preenrollment_general para infos
                .HasForeignKey(i => i.id_data)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar la relación uno-a-uno entre preenrollment_general y preenrollment_tutors
            modelBuilder.Entity<preenrollment_tutors>()
                .HasOne(t => t.preenrollment_general)
                .WithMany() // No hay colección en preenrollment_general para tutors
                .HasForeignKey(t => t.id_data)
                .OnDelete(DeleteBehavior.Cascade);

            // Agregar índices para mejorar el rendimiento
            modelBuilder.Entity<preenrollment_general>()
                .HasIndex(p => p.Curp)
                .IsUnique();

            modelBuilder.Entity<preenrollment_general>()
                .HasIndex(p => p.Email)
                .IsUnique();
        }
    }

           
        }