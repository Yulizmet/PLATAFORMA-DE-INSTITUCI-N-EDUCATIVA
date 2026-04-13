using DocumentFormat.OpenXml.Vml.Office;
using Humanizer.Configuration;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.Models;
using SchoolManager.Models;
using System.Configuration;
using System.Numerics;

namespace SchoolManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        #region DbSets
        // Preenrollment (Inscripciones)
        public DbSet<preenrollment_general> PreenrollmentGenerals { get; set; } = default!;
        public DbSet<preenrollment_addresses> PreenrollmentAddresses { get; set; } = default!;
        public DbSet<preenrollment_careers> PreenrollmentCareers { get; set; } = default!;
        public DbSet<preenrollment_generations> PreenrollmentGenerations { get; set; } = default!;
        public DbSet<preenrollment_infos> PreenrollmentInfos { get; set; } = default!;
        public DbSet<preenrollment_schools> PreenrollmentSchools { get; set; } = default!;
        public DbSet<preenrollment_tutors> PreenrollmentTutors { get; set; } = default!;
        public DbSet<preenrollment_docs> PreenrollmentDocs { get; set; } = default!;



        // Procedures (Trámites)
        public DbSet<procedure_areas> ProcedureAreas { get; set; }
        public DbSet<procedure_documents> ProcedureDocuments { get; set; }
        public DbSet<procedure_flow> ProcedureFlow { get; set; }
        public DbSet<procedure_monitoring> ProcedureMonitoring { get; set; }
        public DbSet<procedure_job_position> ProcedureJobPosition { get; set; }
        public DbSet<procedure_module_catalog> ProcedureModuleCatalog { get; set; }
        public DbSet<procedure_permission> ProcedurePermissions { get; set; }
        public DbSet<procedure_request> ProcedureRequest { get; set; }
        public DbSet<procedure_staff> ProcedureStaff { get; set; }
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

        // Grades (Calificaciones de los compañeros)
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
        public DbSet<grades_enrollment> grades_Enrollments { get; set; }


        // Tutorship (Tutorías)
        public DbSet<tutorship> Tutorships { get; set; }
        public DbSet<tutorship_attendance> TutorshipAttendances { get; set; }
        public DbSet<tutorship_monitoring> TutorshipMonitorings { get; set; }
        public DbSet<tutorship_interview> TutorshipInterviews { get; set; }
        public DbSet<tutorship_interview_answer> TutorshipInterviewAnswers { get; set; }
        public DbSet<tutorship_suggested_topic> TutorshipSuggestedTopics { get; set; }

        // Social Service (Servicio Social)
        public DbSet<social_service_assignment> SocialServiceAssignments { get; set; } = default!;
        public DbSet<social_service_attendance> SocialServiceAttendances { get; set; } = default!;
        public DbSet<social_service_log> SocialServiceLogs { get; set; } = default!;
        public DbSet<social_service_rejection> SocialServiceRejections { get; set; } = default!;

        // Medical (Bitácoras médicas)
        public DbSet<medical_student> MedicalStudents { get; set; }
        public DbSet<medical_logbook> MedicalLogbooks { get; set; }
        public DbSet<medical_pychology> MedicalPsychology { get; set; }
        public DbSet<medical_staff> MedicalStaff { get; set; }
        public DbSet<medical_permissions> MedicalPermissions { get; set; }

        // Foro (Noticias y Publicaciones)
        public DbSet<ForoPublicacion> ForoPublicaciones { get; set; }
        public DbSet<ForoImagen> ForoImagenes { get; set; }
        #endregion


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region 1. Procedures Configuration

            modelBuilder.Entity<procedure_status>().ToTable("procedure_status");
            modelBuilder.Entity<procedure_areas>().ToTable("procedure_areas");
            modelBuilder.Entity<procedure_documents>().ToTable("procedure_documents");
            modelBuilder.Entity<procedure_type_documents>().ToTable("procedure_type_documents");
            modelBuilder.Entity<procedure_type_requirements>().ToTable("procedure_type_requirements");
            modelBuilder.Entity<procedure_permission>().ToTable("procedure_permission");
            modelBuilder.Entity<procedure_types>().ToTable("procedure_types");
            modelBuilder.Entity<procedure_flow>().ToTable("procedure_flow");
            modelBuilder.Entity<procedure_request>().ToTable("procedure_request");
            modelBuilder.Entity<procedure_monitoring>().ToTable("procedure_monitoring");
            modelBuilder.Entity<procedure_job_position>().ToTable("procedure_job_position");
            modelBuilder.Entity<procedure_module_catalog>().ToTable("procedure_module_catalog");

            modelBuilder.Entity<procedure_status>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.InternalCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BackgroundColor).HasMaxLength(20);
                entity.Property(e => e.TextColor).HasMaxLength(20);
                entity.Property(e => e.IsTerminalState).HasDefaultValue(false);
                entity.Property(e => e.IsActionRequiredByUser).HasDefaultValue(false);
                entity.HasIndex(e => e.InternalCode).IsUnique();
            });

            modelBuilder.Entity<procedure_types>(entity => {
                entity.HasOne(d => d.ProcedureArea).WithMany(p => p.ProcedureTypes).HasForeignKey(d => d.IdArea).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<procedure_job_position>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            });

            modelBuilder.Entity<procedure_module_catalog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ModuleName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ButtonName).HasMaxLength(100);
                entity.Property(e => e.Route).HasMaxLength(255);
            });

            modelBuilder.Entity<procedure_permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(d => d.Area).WithMany().HasForeignKey(d => d.IdArea).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_Permission_Area");
                entity.HasOne(d => d.JobPosition).WithMany().HasForeignKey(d => d.IdJobPosition).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_Permission_Job");
                entity.HasOne(d => d.ModuleCatalog).WithMany().HasForeignKey(d => d.IdModuleCatalog).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_Permission_Catalog");
            });

            modelBuilder.Entity<procedure_flow>(entity => {
                entity.Property(e => e.StepOrder).IsRequired();
                entity.HasOne(d => d.ProcedureType).WithMany(p => p.ProcedureFlow).HasForeignKey(d => d.IdTypeProcedure).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.ProcedureStatus).WithMany(p => p.ProcedureFlow).HasForeignKey(d => d.IdStatus).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<procedure_request>(entity => {
                entity.HasOne(d => d.ProcedureType).WithMany(p => p.ProcedureRequests).HasForeignKey(d => d.IdTypeProcedure).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.ProcedureFlow).WithMany().HasForeignKey(d => d.IdProcedureFlow).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.User).WithMany(u => u.ProcedureRequests).HasForeignKey(d => d.IdUser).OnDelete(DeleteBehavior.Restrict);
                entity.Property(p => p.DateCreated).HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
                entity.Property(p => p.DateUpdated).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<procedure_staff>(entity => {
                entity.HasIndex(s => s.IdUser).IsUnique();
                entity.HasOne(s => s.User).WithMany().HasForeignKey(s => s.IdUser).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(s => s.ProcedureArea).WithMany().HasForeignKey(s => s.IdArea).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<procedure_monitoring>(entity => {
                entity.HasOne(pm => pm.ProcedureRequest).WithMany(pr => pr.ProcedureMonitorings).HasForeignKey(pm => pm.IdProcedure).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(pm => pm.ProcedureFlow).WithMany().HasForeignKey(pm => pm.IdProcedureFlow).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(pm => pm.User).WithMany().HasForeignKey(pm => pm.IdUser).OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region 2. Users Configuration
            modelBuilder.Entity<users_person>().ToTable("users_person").HasKey(p => p.PersonId);
            modelBuilder.Entity<users_user>().ToTable("users_user").HasKey(u => u.UserId);
            modelBuilder.Entity<users_role>().ToTable("users_role").HasKey(r => r.RoleId);
            modelBuilder.Entity<users_permission>().ToTable("users_permission").HasKey(p => p.PermissionId);
            modelBuilder.Entity<users_userrole>().ToTable("users_userrole").HasKey(ur => ur.UserRoleId);
            modelBuilder.Entity<users_rolepermission>().ToTable("users_rolepermission").HasKey(rp => rp.RolePermissionId);
            modelBuilder.Entity<users_session>().ToTable("users_session").HasKey(s => s.SessionId);
            modelBuilder.Entity<users_auditlog>().ToTable("users_auditlog").HasKey(a => a.AuditId);

            modelBuilder.Entity<users_user>()
                .HasOne(u => u.Person)
                .WithOne(p => p.User)
                .HasForeignKey<users_user>(u => u.PersonId)
                .OnDelete(DeleteBehavior.Cascade); modelBuilder.Entity<users_userrole>().HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<users_userrole>().HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
            modelBuilder.Entity<users_rolepermission>().HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);
            modelBuilder.Entity<users_session>().HasOne(s => s.User).WithMany(u => u.Sessions).HasForeignKey(s => s.UserId);
            modelBuilder.Entity<users_auditlog>().HasOne(a => a.User).WithMany(u => u.AuditLogs).HasForeignKey(a => a.UserId);
            #endregion

            #region 3. Preenrollment Configuration
            // Configuración de tablas
            modelBuilder.Entity<preenrollment_general>().ToTable("preenrollment_general");
            modelBuilder.Entity<preenrollment_addresses>().ToTable("preenrollment_addresses");
            modelBuilder.Entity<preenrollment_careers>().ToTable("preenrollment_careers");
            modelBuilder.Entity<preenrollment_infos>().ToTable("preenrollment_infos");
            modelBuilder.Entity<preenrollment_schools>().ToTable("preenrollment_schools");
            modelBuilder.Entity<preenrollment_tutors>().ToTable("preenrollment_tutors");
            modelBuilder.Entity<preenrollment_generations>().ToTable("preenrollment_generations");
            modelBuilder.Entity<preenrollment_docs>().ToTable("preenrollment_docs");

            // Índices únicos
            //modelBuilder.Entity<preenrollment_general>()
            //    .HasIndex(p => p.Curp)
            //   .IsUnique();

            //modelBuilder.Entity<preenrollment_general>()
            //    .HasIndex(p => p.Email)
            //    .IsUnique();

      

            // Relación: preenrollment_general -> preenrollment_careers
            modelBuilder.Entity<preenrollment_general>()
                .HasOne(p => p.Career)
                .WithMany(c => c.preenrollment_general)
                .HasForeignKey(p => p.IdCareer)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación: preenrollment_general -> preenrollment_generations
            modelBuilder.Entity<preenrollment_general>()
                .HasOne(p => p.Generation)
                .WithMany(g => g.Students)
                .HasForeignKey(p => p.IdGeneration)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación: preenrollment_general -> users_user (UserId)
            modelBuilder.Entity<preenrollment_general>()
                .HasOne(p => p.User)
                .WithMany(u => u.Preenrollments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //Relación: preenrollment_general -> procedure_request (1:1)
            modelBuilder.Entity<preenrollment_general>()
                .HasOne(p => p.ProcedureRequest)
                .WithMany(r => r.Preenrollments)
                .HasForeignKey(p => p.ProcedureRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relación: preenrollment_addresses -> preenrollment_general
            modelBuilder.Entity<preenrollment_addresses>()
                .HasOne(a => a.General);

            // Relación: preenrollment_schools -> preenrollment_general
            modelBuilder.Entity<preenrollment_schools>()
                .HasOne(s => s.General);

            modelBuilder.Entity<preenrollment_schools>()
                .Property(p => p.average)
                .HasPrecision(5, 2);

            // Relación: preenrollment_infos -> preenrollment_general
            modelBuilder.Entity<preenrollment_infos>()
                .HasOne(i => i.General);

            // Relación: preenrollment_tutors -> preenrollment_general
            modelBuilder.Entity<preenrollment_tutors>()
                .HasOne(t => t.General);

            // Relación: preenrollment_docs -> preenrollment_general
            modelBuilder.Entity<preenrollment_docs>()
                .HasOne(d => d.General)
                .WithMany()  // Asumiendo que es 1:1, pero si es 1:N cambia a .WithMany(g => g.Docs)
                .HasForeignKey(d => d.IdData)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region 4. Grades Configuration

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
            modelBuilder.Entity<grades_enrollment>().ToTable("grades_enrollment");

            // Clave primaria
            modelBuilder.Entity<grades_enrollment>()
                .HasKey(e => e.EnrollmentId);

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


            modelBuilder.Entity<grades_enrollment>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<grades_enrollment>()
                .HasOne(e => e.Group)
                .WithMany(g => g.Enrollments)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<grades_final_grades>()
                .HasOne(f => f.Student)
                .WithMany()
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // grades_grades -> users_user (StudentId)
            modelBuilder.Entity<grades_grades>()
                .HasOne(g => g.Student)
                .WithMany()
                .HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // grades_teacher_subject -> users_user (TeacherId)
            modelBuilder.Entity<grades_teacher_subject>()
                .HasOne(t => t.Teacher)
                .WithMany()
                .HasForeignKey(t => t.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);


            // grades_extraordinary_grades -> grades_final_grades (1:1)
            modelBuilder.Entity<grades_extraordinary_grades>()
                .HasOne(e => e.FinalGrade)
                .WithOne(f => f.ExtraordinaryGrade)
                .HasForeignKey<grades_extraordinary_grades>(e => e.FinalGradeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<grades_final_grades>()
                .HasOne(f => f.ExtraordinaryGrade)
                .WithOne(e => e.FinalGrade)
                .HasForeignKey<grades_extraordinary_grades>(e => e.FinalGradeId);

            // grades_final_grades -> grades_subjects
            modelBuilder.Entity<grades_final_grades>()
                .HasOne(f => f.Subject)
                .WithMany()
                .HasForeignKey(f => f.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // grades_final_grades -> grades_group
            modelBuilder.Entity<grades_final_grades>()
                .HasOne(f => f.Group)
                .WithMany(g => g.FinalGrades)
                .HasForeignKey(f => f.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // grades_grade_level -> grades_group (1:N)
            modelBuilder.Entity<grades_grade_level>()
                .HasMany(g => g.Groups)
                .WithOne(gr => gr.GradeLevel)
                .HasForeignKey(gr => gr.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // grades_grade_level -> grades_subjects (1:N)
            modelBuilder.Entity<grades_grade_level>()
                .HasMany(g => g.Subjects)
                .WithOne(s => s.GradeLevel)
                .HasForeignKey(s => s.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // grades_grades -> grades_group
            modelBuilder.Entity<grades_grades>()
                .HasOne(g => g.Group)
                .WithMany(g => g.Grades)
                .HasForeignKey(g => g.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // grades_grades -> grades_subject_unit
            modelBuilder.Entity<grades_grades>()
                .HasOne(g => g.SubjectUnit)
                .WithMany()
                .HasForeignKey(g => g.SubjectUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // grades_grades -> grades_unit_recovery (1:N)
            modelBuilder.Entity<grades_grades>()
                .HasMany(g => g.Recoveries)
                .WithOne(r => r.Grade)
                .HasForeignKey(r => r.GradeId)
                .OnDelete(DeleteBehavior.Cascade);

            // grades_group -> grades_grade_level
            modelBuilder.Entity<grades_group>()
                .HasOne(g => g.GradeLevel)
                .WithMany(gl => gl.Groups)
                .HasForeignKey(g => g.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // grades_subject_unit -> grades_subjects
            modelBuilder.Entity<grades_subject_unit>()
                .HasOne(su => su.Subject)
                .WithMany(s => s.Units)
                .HasForeignKey(su => su.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // grades_subjects -> grades_grade_level
            modelBuilder.Entity<grades_subjects>()
                .HasOne(s => s.GradeLevel)
                .WithMany(gl => gl.Subjects)
                .HasForeignKey(s => s.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // grades_teacher_subject -> grades_subjects
            modelBuilder.Entity<grades_teacher_subject>()
                .HasOne(t => t.Subject)
                .WithMany()
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // grades_teacher_subject_group -> grades_teacher_subject
            modelBuilder.Entity<grades_teacher_subject_group>()
                .HasOne(t => t.TeacherSubject)
                .WithMany(ts => ts.TeacherSubjectGroups)  
                .HasForeignKey(t => t.TeacherSubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // grades_teacher_subject_group -> grades_group
            modelBuilder.Entity<grades_teacher_subject_group>()
                .HasOne(t => t.Group)
                .WithMany(g => g.TeacherSubjectGroups)
                .HasForeignKey(t => t.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // grades_unit_recovery -> grades_grades
            modelBuilder.Entity<grades_unit_recovery>()
                .HasOne(u => u.Grade)
                .WithMany(g => g.Recoveries)
                .HasForeignKey(u => u.GradeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ÍNDICES
            modelBuilder.Entity<grades_enrollment>()
                .HasIndex(e => new { e.StudentId, e.GroupId })
                .IsUnique();

            modelBuilder.Entity<grades_final_grades>()
                .HasIndex(f => new { f.StudentId, f.SubjectId, f.GroupId });

            modelBuilder.Entity<grades_final_grades>()
                .HasIndex(f => f.StudentId);

            modelBuilder.Entity<grades_grades>()
                .HasIndex(g => new { g.StudentId, g.SubjectUnitId, g.GroupId });

            modelBuilder.Entity<grades_grades>()
                .HasIndex(g => g.StudentId);

            modelBuilder.Entity<grades_subject_unit>()
                .HasIndex(su => new { su.SubjectId, su.UnitNumber })
                .IsUnique();

            modelBuilder.Entity<grades_teacher_subject_group>()
                .HasIndex(t => new { t.TeacherSubjectId, t.GroupId })
                .IsUnique();

            modelBuilder.Entity<grades_group>()
                .HasIndex(g => g.GradeLevelId);

            modelBuilder.Entity<grades_teacher_subject>()
                .HasIndex(t => t.TeacherId);

            // Longitudes y configuraciones de propiedades
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

            // VALORES POR DEFECTO
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

            #endregion

            #region 5. Tutorship Configuration
            modelBuilder.Entity<tutorship>().ToTable("tutorship_sessions");
            modelBuilder.Entity<tutorship_attendance>().ToTable("tutorship_attendances");
            modelBuilder.Entity<tutorship_monitoring>().ToTable("tutorship_monitorings");
            modelBuilder.Entity<tutorship_interview>().ToTable("tutorship_interviews");
            modelBuilder.Entity<tutorship_interview_answer>().ToTable("tutorship_interview_answers");
            modelBuilder.Entity<tutorship_suggested_topic>().ToTable("tutorship_suggested_topic");

            modelBuilder.Entity<tutorship_interview_answer>().HasOne(a => a.Interview).WithMany(i => i.Answers).HasForeignKey(a => a.InterviewId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<tutorship_monitoring>().HasOne(m => m.Student).WithMany().HasForeignKey(m => m.StudentId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tutorship_monitoring>().HasOne(m => m.Teacher).WithMany().HasForeignKey(m => m.TeacherId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tutorship_attendance>().HasOne(a => a.Student).WithMany().HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tutorship_attendance>().HasOne(a => a.Teacher).WithMany().HasForeignKey(a => a.TeacherId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tutorship>().HasOne(t => t.Student).WithMany().HasForeignKey(t => t.StudentId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tutorship>().HasOne(t => t.Teacher).WithMany().HasForeignKey(t => t.TeacherId).OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region 6. Foro Configuration

            modelBuilder.Entity<ForoPublicacion>().ToTable("foro_publicacion");
            modelBuilder.Entity<ForoImagen>().ToTable("foro_imagen");

            modelBuilder.Entity<ForoPublicacion>()
                .HasOne(f => f.Usuario)
                .WithMany(u => u.ForoPublicaciones)
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ForoImagen>()
                .HasOne(i => i.Publicacion)
                .WithMany(p => p.Imagenes)
                .HasForeignKey(i => i.PublicacionId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region 7. Social Service Configuration

            modelBuilder.Entity<social_service_assignment>().ToTable("social_service_assignment");
            modelBuilder.Entity<social_service_attendance>().ToTable("social_service_attendance");
            modelBuilder.Entity<social_service_log>().ToTable("social_service_log");

            // Configuraciones específicas

            modelBuilder.Entity<social_service_assignment>(entity =>
            {
                entity.HasOne(e => e.Student)
                    .WithMany()
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Teacher)
                    .WithMany()
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<social_service_rejection>(entity =>
            {
                entity.HasOne<users_user>()
                    .WithMany()
                    .HasForeignKey(e => e.RejectedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Student)
                    .WithMany()
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<social_service_assignment>()
                .HasIndex(a => new { a.TeacherId, a.StudentId })
                .IsUnique();

            modelBuilder.Entity<social_service_assignment>()
                .Property(a => a.AssignedDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<social_service_attendance>()
                .Property(a => a.Tipo)
                .HasDefaultValue("Servicio Social");

            modelBuilder.Entity<social_service_log>()
                .Property(l => l.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // Índices para mejorar búsquedas frecuentes
            modelBuilder.Entity<social_service_attendance>()
                .HasIndex(a => new { a.StudentId, a.Date });

            modelBuilder.Entity<social_service_log>()
                .HasIndex(l => new { l.StudentId, l.Week })
                .IsUnique();

            #endregion

            #region 8. Medical Configuration
            modelBuilder.Entity<medical_student>().ToTable("medical_students");
            modelBuilder.Entity<medical_logbook>().ToTable("medical_records");
            modelBuilder.Entity<medical_pychology>().ToTable("medical_psychology_appointments");
            modelBuilder.Entity<medical_staff>().ToTable("medical_staff");

            #endregion

        }
    }
}