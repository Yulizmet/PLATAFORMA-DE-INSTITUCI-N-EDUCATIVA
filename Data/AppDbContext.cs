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

        public DbSet<grades_extraordinary_grades> grades_ExtraordinaryGrades { get; set; }
        public DbSet<grades_final_grades> grades_FinalGrades { get; set; }
        public DbSet<grades_grade_level> grades_GradeLevels { get; set; }
        public DbSet<grades_grades> grades_Grades { get; set; }
        public DbSet<grades_group> grades_GradeGroups { get; set; }
        public DbSet<grades_school_cycle> grades_SchoolCycles { get; set; }
        public DbSet<grades_subject_unit> grades_SubjectUnits { get; set; }
        public DbSet<grades_subjects> grades_Subjects { get; set; }
        public DbSet<grades_teacher_subject> grades_TeacherSubjects { get; set; }
        public DbSet<grades_teacher_subject_group> grades_TeacherSubjectGroups { get; set; }
        public DbSet<grades_unit_recovery> grades_UnitRecoveries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de claves primarias
            modelBuilder.Entity<grades_extraordinary_grades>()
                .HasKey(e => e.ExtraordinaryGradeId);

            modelBuilder.Entity<grades_final_grades>()
                .HasKey(f => f.FinalGradeId);

            modelBuilder.Entity<grades_grade_level>()
                .HasKey(g => g.GradeLevelId);

            modelBuilder.Entity<grades_grades>()
                .HasKey(g => g.GradeId);

            modelBuilder.Entity<grades_group>()
                .HasKey(g => g.GroupId);

            modelBuilder.Entity<grades_school_cycle>()
                .HasKey(s => s.SchoolCycleId);

            modelBuilder.Entity<grades_subject_unit>()
                .HasKey(s => s.UnitId);

            modelBuilder.Entity<grades_subjects>()
                .HasKey(s => s.SubjectId);

            modelBuilder.Entity<grades_teacher_subject>()
                .HasKey(t => t.TeacherSubjectId);

            modelBuilder.Entity<grades_teacher_subject_group>()
                .HasKey(t => t.TeacherSubjectGroupId);

            modelBuilder.Entity<grades_unit_recovery>()
                .HasKey(u => u.UnitRecoveryId);

            // Relaciones

            // 1. grades_extraordinary_grades -> grades_final_grades (1:1)
            modelBuilder.Entity<grades_extraordinary_grades>()
                .HasOne(e => e.FinalGrade)
                .WithOne(f => f.ExtraordinaryGrade)
                .HasForeignKey<grades_extraordinary_grades>(e => e.FinalGradeId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. grades_final_grades -> grades_subjects
            modelBuilder.Entity<grades_final_grades>()
                .HasOne(f => f.Subject)
                .WithMany()
                .HasForeignKey(f => f.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. grades_final_grades -> grades_group
            modelBuilder.Entity<grades_final_grades>()
                .HasOne(f => f.grades_group)
                .WithMany()
                .HasForeignKey(f => f.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. grades_grade_level -> grades_group (1:N)
            modelBuilder.Entity<grades_grade_level>()
                .HasMany(g => g.Groups)
                .WithOne(gr => gr.GradeLevel)
                .HasForeignKey(gr => gr.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. grades_grade_level -> grades_subjects (1:N)
            modelBuilder.Entity<grades_grade_level>()
                .HasMany(g => g.Subjects)
                .WithOne(s => s.GradeLevel)
                .HasForeignKey(s => s.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. grades_grades -> grades_group
            modelBuilder.Entity<grades_grades>()
                .HasOne(g => g.grades_group)
                .WithMany()
                .HasForeignKey(g => g.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // 7. grades_grades -> grades_subject_unit
            modelBuilder.Entity<grades_grades>()
                .HasOne(g => g.SubjectUnit)
                .WithMany()
                .HasForeignKey(g => g.SubjectUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // 8. grades_grades -> grades_unit_recovery (1:N)
            modelBuilder.Entity<grades_grades>()
                .HasMany(g => g.Recoveries)
                .WithOne(r => r.Grade)
                .HasForeignKey(r => r.GradeId)
                .OnDelete(DeleteBehavior.Cascade);

            // 9. grades_group -> grades_grade_level
            modelBuilder.Entity<grades_group>()
                .HasOne(g => g.GradeLevel)
                .WithMany(gl => gl.Groups)
                .HasForeignKey(g => g.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // 10. grades_group -> grades_school_cycle
            modelBuilder.Entity<grades_group>()
                .HasOne(g => g.SchoolCycle)
                .WithMany(sc => sc.Groups)
                .HasForeignKey(g => g.SchoolCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            // 11. grades_school_cycle -> grades_group
            modelBuilder.Entity<grades_school_cycle>()
                .HasMany(s => s.Groups)
                .WithOne(g => g.SchoolCycle)
                .HasForeignKey(g => g.SchoolCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            // 12. grades_subject_unit -> grades_subjects
            modelBuilder.Entity<grades_subject_unit>()
                .HasOne(su => su.Subject)
                .WithMany(s => s.Units)
                .HasForeignKey(su => su.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // 13. grades_subjects -> grades_grade_level
            modelBuilder.Entity<grades_subjects>()
                .HasOne(s => s.GradeLevel)
                .WithMany(gl => gl.Subjects)
                .HasForeignKey(s => s.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // 14. grades_teacher_subject -> grades_subjects
            modelBuilder.Entity<grades_teacher_subject>()
                .HasOne(t => t.Subject)
                .WithMany()
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // 15. grades_teacher_subject_group -> grades_teacher_subject
            modelBuilder.Entity<grades_teacher_subject_group>()
                .HasOne(t => t.TeacherSubject)
                .WithMany()
                .HasForeignKey(t => t.TeacherSubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // 16. grades_teacher_subject_group -> grades_group
            modelBuilder.Entity<grades_teacher_subject_group>()
                .HasOne(t => t.grades_group)
                .WithMany()
                .HasForeignKey(t => t.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // 17. grades_unit_recovery -> grades_grades
            modelBuilder.Entity<grades_unit_recovery>()
                .HasOne(u => u.Grade)
                .WithMany(g => g.Recoveries)
                .HasForeignKey(u => u.GradeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de índices para mejor rendimiento

            // Para búsquedas frecuentes de calificaciones finales
            modelBuilder.Entity<grades_final_grades>()
                .HasIndex(f => new { f.StudentId, f.SubjectId, f.GroupId });

            // Para búsquedas frecuentes de calificaciones de unidades
            modelBuilder.Entity<grades_grades>()
                .HasIndex(g => new { g.StudentId, g.SubjectUnitId, g.GroupId });

            // Para asegurar unicidad en unidades por materia
            modelBuilder.Entity<grades_subject_unit>()
                .HasIndex(su => new { su.SubjectId, su.UnitNumber })
                .IsUnique();

            // Para asegurar que un profesor no tenga duplicados en grupos
            modelBuilder.Entity<grades_teacher_subject_group>()
                .HasIndex(t => new { t.TeacherSubjectId, t.GroupId })
                .IsUnique();

            // Índices adicionales recomendados
            modelBuilder.Entity<grades_final_grades>()
                .HasIndex(f => f.StudentId);

            modelBuilder.Entity<grades_grades>()
                .HasIndex(g => g.StudentId);

            modelBuilder.Entity<grades_group>()
                .HasIndex(g => g.GradeLevelId);

            modelBuilder.Entity<grades_group>()
                .HasIndex(g => g.SchoolCycleId);

            // Configuración de tipos de datos y límites
            modelBuilder.Entity<grades_grade_level>()
                .Property(g => g.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<grades_group>()
                .Property(g => g.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<grades_subjects>()
                .Property(s => s.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<grades_school_cycle>()
                .Property(s => s.Name)
                .HasMaxLength(100)
                .IsRequired();

            // Configuración de valores por defecto
            modelBuilder.Entity<grades_final_grades>()
                .Property(f => f.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<grades_grades>()
                .Property(g => g.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<grades_extraordinary_grades>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<grades_unit_recovery>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");



            modelBuilder.Entity<procedure_status>().ToTable("procedure_status");
            modelBuilder.Entity<procedure_areas>().ToTable("procedure_areas");
            modelBuilder.Entity<procedure_documents>().ToTable("procedure_documents");
            modelBuilder.Entity<procedure_request>().ToTable("procedure_request");
            modelBuilder.Entity<procedure_type_documents>().ToTable("procedure_type_documents");
            modelBuilder.Entity<procedure_type_requirements>().ToTable("procedure_type_requirements");
            modelBuilder.Entity<procedure_types>().ToTable("procedure_types");
            modelBuilder.Entity<procedure_monitoring>().ToTable("procedure_monitoring");
            // Configurar nombres de tabla para grades
            modelBuilder.Entity<grades_extraordinary_grades>().ToTable("grades_extraordinary_grades");
            modelBuilder.Entity<grades_final_grades>().ToTable("grades_final_grades");
            modelBuilder.Entity<grades_grade_level>().ToTable("grades_grade_level");
            modelBuilder.Entity<grades_grades>().ToTable("grades_grades");
            modelBuilder.Entity<grades_group>().ToTable("grades_group");
            modelBuilder.Entity<grades_school_cycle>().ToTable("grades_school_cycle");
            modelBuilder.Entity<grades_subject_unit>().ToTable("grades_subject_unit");
            modelBuilder.Entity<grades_subjects>().ToTable("grades_subjects");
            modelBuilder.Entity<grades_teacher_subject>().ToTable("grades_teacher_subject");
            modelBuilder.Entity<grades_teacher_subject_group>().ToTable("grades_teacher_subject_group");
            modelBuilder.Entity<grades_unit_recovery>().ToTable("grades_unit_recovery");
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
