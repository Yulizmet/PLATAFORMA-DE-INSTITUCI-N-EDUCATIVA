using Microsoft.EntityFrameworkCore;
using SchoolManager.Models;

namespace SchoolManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        #region DbSets
        // Preenrollment (Inscripciones)
        public DbSet<preenrollment_general> PreenrollmentGenerals { get; set; } = default!;
        public DbSet<preenrollment_addresses> PreenrollmentAddresses { get; set; } = default!;
        public DbSet<preenrollment_careers> PreenrollmentCareers { get; set; } = default!;
        public DbSet<preenrollment_generations> PreenrollmentGenerations { get; set; } = default!;
        public DbSet<preenrollment_infos> PreenrollmentInfos { get; set; } = default!;
        public DbSet<preenrollment_schools> PreenrollmentSchools { get; set; } = default!;
        public DbSet<preenrollment_tutors> PreenrollmentTutors { get; set; } = default!;

        // Procedures (Trámites)
        public DbSet<procedure_areas> ProcedureAreas { get; set; }
        public DbSet<procedure_documents> ProcedureDocuments { get; set; }
        public DbSet<procedure_flow> ProcedureFlow { get; set; }
        public DbSet<procedure_monitoring> ProcedureMonitoring { get; set; }
        public DbSet<procedure_request> ProcedureRequest { get; set; }
        public DbSet<procedure_status> ProcedureStatus { get; set; }
        public DbSet<procedure_type_documents> ProcedureTypeDocuments { get; set; }
        public DbSet<procedure_type_requirements> ProcedureTypeRequirements { get; set; }
        public DbSet<procedure_types> ProcedureTypes { get; set; }

        // Users (Gestión de Usuarios)
        public DbSet<users_person> Persons { get; set; }
        public DbSet<users_user> Users { get; set; }
        public DbSet<users_auditlog> AuditLogs { get; set; }
        public DbSet<users_permission> Permissions { get; set; }
        public DbSet<users_role> Roles { get; set; }
        public DbSet<users_userrole> UserRoles { get; set; }
        public DbSet<users_rolepermission> RolePermissions { get; set; }
        public DbSet<users_session> Sessions { get; set; }

        // Grades (Calificaciones)
        public DbSet<grades_extraordinary_grades> grades_ExtraordinaryGrades { get; set; }
        public DbSet<grades_final_grades> grades_FinalGrades { get; set; }
        public DbSet<grades_grade_level> grades_GradeLevels { get; set; }
        public DbSet<grades_grades> grades_Grades { get; set; }
        public DbSet<grades_group> grades_GradeGroups { get; set; }
        public DbSet<grades_subject_unit> grades_SubjectUnits { get; set; }
        public DbSet<grades_subjects> grades_Subjects { get; set; }
        public DbSet<grades_teacher_subject> grades_TeacherSubjects { get; set; }
        public DbSet<grades_teacher_subject_group> grades_TeacherSubjectGroups { get; set; }
        public DbSet<grades_unit_recovery> grades_UnitRecoveries { get; set; }

        // Tutorship (Tutorías)
        public DbSet<tutorship> Tutorships { get; set; }
        public DbSet<tutorship_attendance> TutorshipAttendances { get; set; }
        public DbSet<tutorship_monitoring> TutorshipMonitorings { get; set; }
        public DbSet<tutorship_interview> TutorshipInterviews { get; set; }
        public DbSet<tutorship_interview_answer> TutorshipInterviewAnswers { get; set; }
        #endregion

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
                .HasOne<grades_group>()  
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
                .HasOne<grades_group>()
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


            modelBuilder.Entity<grades_grade_level>()
                .Property(g => g.StartDate)
                .IsRequired();

            modelBuilder.Entity<grades_grade_level>()
                .Property(g => g.EndDate)
                .IsRequired();

            modelBuilder.Entity<grades_grade_level>()
                .Property(g => g.IsOpen)
                .HasDefaultValue(true);

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
                .HasOne<grades_group>()
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

            modelBuilder.Entity<Generation>().ToTable("Generation");

            //Procedures
            #region 1. Procedures Configuration
            modelBuilder.Entity<procedure_status>().ToTable("procedure_status");
            #region 1. Users Configuration
            modelBuilder.Entity<users_person>().ToTable("users_person").HasKey(p => p.PersonId);
            modelBuilder.Entity<users_user>().ToTable("users_user").HasKey(u => u.UserId);
            modelBuilder.Entity<users_role>().ToTable("users_role").HasKey(r => r.RoleId);
            modelBuilder.Entity<users_permission>().ToTable("users_permission").HasKey(p => p.PermissionId);
            modelBuilder.Entity<users_userrole>().ToTable("users_userrole").HasKey(ur => ur.UserRoleId);
            modelBuilder.Entity<users_rolepermission>().ToTable("users_rolepermission").HasKey(rp => rp.RolePermissionId);
            modelBuilder.Entity<users_session>().ToTable("users_session").HasKey(s => s.SessionId);
            modelBuilder.Entity<users_auditlog>().ToTable("users_auditlog").HasKey(a => a.AuditId);

            modelBuilder.Entity<users_person>().HasOne(p => p.User).WithOne(u => u.Person).HasForeignKey<users_user>(u => u.PersonId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<users_userrole>().HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<users_userrole>().HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<users_rolepermission>().HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<users_session>().HasOne(s => s.User).WithMany(u => u.Sessions).HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<users_auditlog>().HasOne(a => a.User).WithMany(u => u.AuditLogs).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region 2. Procedures Configuration
            modelBuilder.Entity<procedure_status>().ToTable("procedure_status").HasKey(e => e.Id);
            modelBuilder.Entity<procedure_areas>().ToTable("procedure_areas");
            modelBuilder.Entity<procedure_documents>().ToTable("procedure_documents");
            modelBuilder.Entity<procedure_type_documents>().ToTable("procedure_type_documents");
            modelBuilder.Entity<procedure_type_requirements>().ToTable("procedure_type_requirements");

            modelBuilder.Entity<procedure_status>(entity => {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.InternalCode).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.InternalCode).IsUnique();
            });

            modelBuilder.Entity<procedure_types>(entity => {
                entity.ToTable("procedure_types");
                entity.HasOne(d => d.ProcedureArea).WithMany(p => p.ProcedureTypes).HasForeignKey(d => d.IdArea);
            });

            modelBuilder.Entity<procedure_flow>(entity => {
                entity.ToTable("procedure_flow");
                entity.HasOne(d => d.ProcedureType).WithMany(p => p.ProcedureFlow).HasForeignKey(d => d.IdTypeProcedure).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.ProcedureStatus).WithMany(p => p.ProcedureFlow).HasForeignKey(d => d.IdStatus).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<procedure_request>(entity => {
                entity.ToTable("procedure_request");
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.IdUser).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.ProcedureType).WithMany(p => p.ProcedureRequests).HasForeignKey(d => d.IdTypeProcedure).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.ProcedureFlow).WithMany().HasForeignKey(d => d.IdProcedureFlow).OnDelete(DeleteBehavior.Restrict);
                entity.Property(p => p.DateCreated).HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<procedure_monitoring>(entity => {
                entity.ToTable("procedure_monitoring");
                entity.HasOne(pm => pm.ProcedureRequest).WithMany(pr => pr.ProcedureMonitorings).HasForeignKey(pm => pm.IdProcedure).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(pm => pm.ProcedureFlow).WithMany().HasForeignKey(pm => pm.IdProcedureFlow).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(pm => pm.User).WithMany().HasForeignKey(pm => pm.IdUser).OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

            #region 3. Preenrollment Configuration
            modelBuilder.Entity<preenrollment_general>().ToTable("preenrollment_general");
            modelBuilder.Entity<preenrollment_generations>().ToTable("preenrollment_generations");

            modelBuilder.Entity<preenrollment_addresses>().ToTable("preenrollment_addresses")
                .HasOne(a => a.preenrollment_general).WithMany(g => g.Addresses).HasForeignKey(a => a.id_data).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<preenrollment_schools>().ToTable("preenrollment_schools")
                .HasOne(s => s.preenrollment_general).WithMany(g => g.Schools).HasForeignKey(s => s.id_data).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<preenrollment_infos>().ToTable("preenrollment_infos")
                .HasOne(i => i.preenrollment_general).WithMany().HasForeignKey(i => i.id_data).OnDelete(DeleteBehavior.Cascade);

            #region 3. Preenrollment Configuration
            modelBuilder.Entity<preenrollment_general>().ToTable("preenrollment_general");
            modelBuilder.Entity<preenrollment_addresses>().ToTable("preenrollment_addresses");
            modelBuilder.Entity<preenrollment_careers>().ToTable("preenrollment_careers");
            modelBuilder.Entity<preenrollment_infos>().ToTable("preenrollment_infos");
            modelBuilder.Entity<preenrollment_schools>().ToTable("preenrollment_schools");
            modelBuilder.Entity<preenrollment_tutors>().ToTable("preenrollment_tutors");

            modelBuilder.Entity<users_auditlog>()
                .HasKey(a => a.AuditId);

            modelBuilder.Entity<users_session>()
                .HasKey(s => s.SessionId);
            modelBuilder.Entity<users_permission>()
                .HasKey(s => s.PermissionId);
            modelBuilder.Entity<users_person>()
                .HasKey(s => s.PersonId);
            modelBuilder.Entity<users_role>()
                .HasKey(s => s.RoleId);
            modelBuilder.Entity<users_rolepermission>()
                .HasKey(s => s.RolePermissionId);
            modelBuilder.Entity<users_user>()
                .HasKey(s => s.UserId);
            modelBuilder.Entity<users_userrole>()
                .HasKey(s => s.UserRoleId);
            // Configurar nombres de tabla para grades
            modelBuilder.Entity<grades_extraordinary_grades>().ToTable("grades_extraordinary_grades");
            modelBuilder.Entity<grades_final_grades>().ToTable("grades_final_grades");
            modelBuilder.Entity<grades_grade_level>().ToTable("grades_grade_level");
            modelBuilder.Entity<grades_grades>().ToTable("grades_grades");
            modelBuilder.Entity<grades_group>().ToTable("grades_group");
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

            // Relaciones de Usuarios
            modelBuilder.Entity<users_person>()
                .HasOne(p => p.User)
                .WithOne(u => u.Person)
                .HasForeignKey<users_user>(u => u.PersonId)
                .OnDelete(DeleteBehavior.Cascade);  












            modelBuilder.Entity<users_userrole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<users_userrole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<users_rolepermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<users_session>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<users_auditlog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);





            // Configurar la relación entre preenrollment_general y Generations
            modelBuilder.Entity<preenrollment_general>()
                .HasOne(p => p.Generation)
                .WithMany(g => g.Students)
                .HasForeignKey(p => p.IdGeneration)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<preenrollment_general>().HasIndex(p => p.Curp).IsUnique();
            modelBuilder.Entity<preenrollment_general>().HasIndex(p => p.Email).IsUnique();
            modelBuilder.Entity<Generation>().ToTable("Generation");
            modelBuilder.Entity<preenrollment_tutors>().ToTable("preenrollment_tutors")
                .HasOne(t => t.preenrollment_general).WithMany().HasForeignKey(t => t.id_data).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<preenrollment_general>().HasOne(p => p.Generation).WithMany(g => g.Students).HasForeignKey(p => p.IdGeneration).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<preenrollment_general>().HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<preenrollment_general>().HasOne(p => p.ProcedureRequest).WithMany().HasForeignKey(p => p.ProcedureRequestId).OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region 4. Grades Configuration
            modelBuilder.Entity<grades_extraordinary_grades>().ToTable("grades_extraordinary_grades").HasKey(e => e.ExtraordinaryGradeId);
            modelBuilder.Entity<grades_final_grades>().ToTable("grades_final_grades").HasKey(f => f.FinalGradeId);
            modelBuilder.Entity<grades_grade_level>().ToTable("grades_grade_level").HasKey(g => g.GradeLevelId);
            modelBuilder.Entity<grades_grades>().ToTable("grades_grades").HasKey(g => g.GradeId);
            modelBuilder.Entity<grades_group>().ToTable("grades_group").HasKey(g => g.GroupId);
            modelBuilder.Entity<grades_school_cycle>().ToTable("grades_school_cycle").HasKey(s => s.SchoolCycleId);
            modelBuilder.Entity<grades_subject_unit>().ToTable("grades_subject_unit").HasKey(s => s.UnitId);
            modelBuilder.Entity<grades_subjects>().ToTable("grades_subjects").HasKey(s => s.SubjectId);
            modelBuilder.Entity<grades_teacher_subject>().ToTable("grades_teacher_subject").HasKey(t => t.TeacherSubjectId);
            modelBuilder.Entity<grades_teacher_subject_group>().ToTable("grades_teacher_subject_group").HasKey(t => t.TeacherSubjectGroupId);
            modelBuilder.Entity<grades_unit_recovery>().ToTable("grades_unit_recovery").HasKey(u => u.UnitRecoveryId);

            // Relaciones
            modelBuilder.Entity<grades_extraordinary_grades>().HasOne(e => e.FinalGrade).WithOne(f => f.ExtraordinaryGrade).HasForeignKey<grades_extraordinary_grades>(e => e.FinalGradeId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<grades_final_grades>().HasOne(f => f.Subject).WithMany().HasForeignKey(f => f.SubjectId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<grades_final_grades>().HasOne(f => f.grades_group).WithMany().HasForeignKey(f => f.GroupId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<grades_grade_level>().HasMany(g => g.Groups).WithOne(gr => gr.GradeLevel).HasForeignKey(gr => gr.GradeLevelId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<grades_grade_level>().HasMany(g => g.Subjects).WithOne(s => s.GradeLevel).HasForeignKey(s => s.GradeLevelId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<grades_grades>().HasOne(g => g.grades_group).WithMany().HasForeignKey(g => g.GroupId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<grades_grades>().HasOne(g => g.SubjectUnit).WithMany().HasForeignKey(g => g.SubjectUnitId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<grades_grades>().HasMany(g => g.Recoveries).WithOne(r => r.Grade).HasForeignKey(r => r.GradeId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<grades_group>().HasOne(g => g.GradeLevel).WithMany(gl => gl.Groups).HasForeignKey(g => g.GradeLevelId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<grades_group>().HasOne(g => g.SchoolCycle).WithMany(sc => sc.Groups).HasForeignKey(g => g.SchoolCycleId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<grades_subject_unit>().HasOne(su => su.Subject).WithMany(s => s.Units).HasForeignKey(su => su.SubjectId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<grades_subjects>().HasOne(s => s.GradeLevel).WithMany(gl => gl.Subjects).HasForeignKey(s => s.GradeLevelId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<grades_teacher_subject>().HasOne(t => t.Subject).WithMany().HasForeignKey(t => t.SubjectId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<grades_teacher_subject_group>().HasOne(t => t.TeacherSubject).WithMany().HasForeignKey(t => t.TeacherSubjectId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<grades_teacher_subject_group>().HasOne(t => t.grades_group).WithMany().HasForeignKey(t => t.GroupId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<grades_unit_recovery>().HasOne(u => u.Grade).WithMany(g => g.Recoveries).HasForeignKey(u => u.GradeId).OnDelete(DeleteBehavior.Cascade);

            // Índices
            modelBuilder.Entity<grades_final_grades>().HasIndex(f => new { f.StudentId, f.SubjectId, f.GroupId });
            modelBuilder.Entity<grades_grades>().HasIndex(g => new { g.StudentId, g.SubjectUnitId, g.GroupId });
            modelBuilder.Entity<grades_subject_unit>().HasIndex(su => new { su.SubjectId, su.UnitNumber }).IsUnique();
            modelBuilder.Entity<grades_teacher_subject_group>().HasIndex(t => new { t.TeacherSubjectId, t.GroupId }).IsUnique();
            #endregion

            #region 5. Tutorship Configuration
            modelBuilder.Entity<tutorship>().ToTable("tutorship_sessions");
            modelBuilder.Entity<tutorship_attendance>().ToTable("tutorship_attendances");
            modelBuilder.Entity<tutorship_monitoring>().ToTable("tutorship_monitorings");
            modelBuilder.Entity<tutorship_interview>().ToTable("tutorship_interviews");
            modelBuilder.Entity<tutorship_interview_answer>().ToTable("tutorship_interview_answers");

            modelBuilder.Entity<tutorship_interview_answer>().HasOne(a => a.Interview).WithMany(i => i.Answers).HasForeignKey(a => a.InterviewId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<tutorship_monitoring>().HasOne(m => m.Student).WithMany().HasForeignKey(m => m.StudentId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tutorship_monitoring>().HasOne(m => m.Teacher).WithMany().HasForeignKey(m => m.TeacherId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tutorship_attendance>().HasOne(a => a.Student).WithMany().HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tutorship_attendance>().HasOne(a => a.Teacher).WithMany().HasForeignKey(a => a.TeacherId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tutorship>().HasOne(t => t.Student).WithMany().HasForeignKey(t => t.StudentId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tutorship>().HasOne(t => t.Teacher).WithMany().HasForeignKey(t => t.TeacherId).OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}