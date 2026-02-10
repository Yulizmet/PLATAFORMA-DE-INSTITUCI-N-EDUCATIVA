using Microsoft.EntityFrameworkCore;
using SchoolManager.Models;
using System;
using System.Data;
using System.Security;

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

        public DbSet<users_person> Persons {  get; set; }
        public DbSet<users_user> Users { get; set; }
        public DbSet<users_auditlog> AuditLogs { get; set; }
        public DbSet<users_permission> Permissions { get; set; }
        public DbSet<users_role> Roles { get; set; }
        public DbSet<users_userrole> UserRoles { get; set; }
        public DbSet<users_rolepermission> RolePermissions { get; set; }
        public DbSet<users_session> Sessions { get; set; }

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

            ///UsersCategory
            modelBuilder.Entity<users_person>().ToTable("users_person");
            modelBuilder.Entity<users_user>().ToTable("users_user");
            modelBuilder.Entity<users_role>().ToTable("users_role");
            modelBuilder.Entity<users_permission>().ToTable("users_permission");
            modelBuilder.Entity<users_userrole>().ToTable("users_userrole");
            modelBuilder.Entity<users_rolepermission>().ToTable("users_rolepermission");
            modelBuilder.Entity<users_session>().ToTable("users_session");
            modelBuilder.Entity<users_auditlog>().ToTable("users_auditlog");

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

            ///UsersCategory
            modelBuilder.Entity<users_person>()
                .HasOne(p => p.User)
                .WithOne(u => u.Person)
                .HasForeignKey<users_user>(u => u.PersonId);

            modelBuilder.Entity<users_userrole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<users_userrole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<users_rolepermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<users_rolepermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
        }
    }
}
