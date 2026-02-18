using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManager.Migrations
{
    /// <inheritdoc />
    public partial class AddEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grades_grade_level",
                columns: table => new
                {
                    GradeLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_grade_level", x => x.GradeLevelId);
                });

            migrationBuilder.CreateTable(
                name: "grades_school_cycle",
                columns: table => new
                {
                    SchoolCycleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_school_cycle", x => x.SchoolCycleId);
                });

            migrationBuilder.CreateTable(
                name: "preenrollment_careers",
                columns: table => new
                {
                    IdCareer = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_career = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preenrollment_careers", x => x.IdCareer);
                });

            migrationBuilder.CreateTable(
                name: "procedure_areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedure_areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "procedure_status",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    TextColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedure_status", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "procedure_type_documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedure_type_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users_permission",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_permission", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "users_person",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastNamePaternal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastNameMaternal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Curp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_person", x => x.PersonId);
                });

            migrationBuilder.CreateTable(
                name: "users_role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "grades_subjects",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GradeLevelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_subjects", x => x.SubjectId);
                    table.ForeignKey(
                        name: "FK_grades_subjects_grades_grade_level_GradeLevelId",
                        column: x => x.GradeLevelId,
                        principalTable: "grades_grade_level",
                        principalColumn: "GradeLevelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grades_group",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GradeLevelId = table.Column<int>(type: "int", nullable: false),
                    SchoolCycleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_group", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_grades_group_grades_grade_level_GradeLevelId",
                        column: x => x.GradeLevelId,
                        principalTable: "grades_grade_level",
                        principalColumn: "GradeLevelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grades_group_grades_school_cycle_SchoolCycleId",
                        column: x => x.SchoolCycleId,
                        principalTable: "grades_school_cycle",
                        principalColumn: "SchoolCycleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "preenrollment_general",
                columns: table => new
                {
                    IdData = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCareer = table.Column<int>(type: "int", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Work = table.Column<bool>(type: "bit", nullable: false),
                    Occupation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WorkAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WorkPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Folio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreateStat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BloodType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    preenrollment_careersIdCareer = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preenrollment_general", x => x.IdData);
                    table.ForeignKey(
                        name: "FK_preenrollment_general_preenrollment_careers_preenrollment_careersIdCareer",
                        column: x => x.preenrollment_careersIdCareer,
                        principalTable: "preenrollment_careers",
                        principalColumn: "IdCareer");
                });

            migrationBuilder.CreateTable(
                name: "procedure_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdArea = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedure_types", x => x.Id);
                    table.ForeignKey(
                        name: "FK_procedure_types_procedure_areas_IdArea",
                        column: x => x.IdArea,
                        principalTable: "procedure_areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_user",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_user", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_users_user_users_person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "users_person",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_rolepermission",
                columns: table => new
                {
                    RolePermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_rolepermission", x => x.RolePermissionId);
                    table.ForeignKey(
                        name: "FK_users_rolepermission_users_permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "users_permission",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_users_rolepermission_users_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "users_role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grades_subject_unit",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    UnitNumber = table.Column<int>(type: "int", nullable: false),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_subject_unit", x => x.UnitId);
                    table.ForeignKey(
                        name: "FK_grades_subject_unit_grades_subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "grades_subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grades_teacher_subject",
                columns: table => new
                {
                    TeacherSubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_teacher_subject", x => x.TeacherSubjectId);
                    table.ForeignKey(
                        name: "FK_grades_teacher_subject_grades_subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "grades_subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grades_final_grades",
                columns: table => new
                {
                    FinalGradeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Passed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_final_grades", x => x.FinalGradeId);
                    table.ForeignKey(
                        name: "FK_grades_final_grades_grades_group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "grades_group",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grades_final_grades_grades_subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "grades_subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "preenrollment_addresses",
                columns: table => new
                {
                    IdAddress = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_data = table.Column<int>(type: "int", nullable: false),
                    street = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    exterior_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    interior_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    postal_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    neighborhood = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    state = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preenrollment_addresses", x => x.IdAddress);
                    table.ForeignKey(
                        name: "FK_preenrollment_addresses_preenrollment_general_id_data",
                        column: x => x.id_data,
                        principalTable: "preenrollment_general",
                        principalColumn: "IdData",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preenrollment_infos",
                columns: table => new
                {
                    IdInfo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_data = table.Column<int>(type: "int", nullable: false),
                    beca = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    comu_indi = table.Column<bool>(type: "bit", nullable: false),
                    lengu_indi = table.Column<bool>(type: "bit", nullable: false),
                    incapa = table.Column<bool>(type: "bit", nullable: false),
                    disease = table.Column<bool>(type: "bit", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preenrollment_infos", x => x.IdInfo);
                    table.ForeignKey(
                        name: "FK_preenrollment_infos_preenrollment_general_id_data",
                        column: x => x.id_data,
                        principalTable: "preenrollment_general",
                        principalColumn: "IdData",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preenrollment_schools",
                columns: table => new
                {
                    IdSchool = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_data = table.Column<int>(type: "int", nullable: false),
                    school = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    degree = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    state = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    average = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    study_system = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    high_school_type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preenrollment_schools", x => x.IdSchool);
                    table.ForeignKey(
                        name: "FK_preenrollment_schools_preenrollment_general_id_data",
                        column: x => x.id_data,
                        principalTable: "preenrollment_general",
                        principalColumn: "IdData",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preenrollment_tutors",
                columns: table => new
                {
                    IdTutor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_data = table.Column<int>(type: "int", nullable: false),
                    relationship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    paternal_last_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    maternal_last_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    home_phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    work_phone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preenrollment_tutors", x => x.IdTutor);
                    table.ForeignKey(
                        name: "FK_preenrollment_tutors_preenrollment_general_id_data",
                        column: x => x.id_data,
                        principalTable: "preenrollment_general",
                        principalColumn: "IdData",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "procedure_request",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Folio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdTypeProcedure = table.Column<int>(type: "int", nullable: false),
                    IdStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedure_request", x => x.Id);
                    table.ForeignKey(
                        name: "FK_procedure_request_procedure_status_IdStatus",
                        column: x => x.IdStatus,
                        principalTable: "procedure_status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_procedure_request_procedure_types_IdTypeProcedure",
                        column: x => x.IdTypeProcedure,
                        principalTable: "procedure_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "procedure_type_requirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTypeProcedure = table.Column<int>(type: "int", nullable: false),
                    IdTypeDocument = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedure_type_requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_procedure_type_requirements_procedure_type_documents_IdTypeDocument",
                        column: x => x.IdTypeDocument,
                        principalTable: "procedure_type_documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_procedure_type_requirements_procedure_types_IdTypeProcedure",
                        column: x => x.IdTypeProcedure,
                        principalTable: "procedure_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tutorship_attendances",
                columns: table => new
                {
                    AttendanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tutorship_attendances", x => x.AttendanceId);
                    table.ForeignKey(
                        name: "FK_tutorship_attendances_users_user_StudentId",
                        column: x => x.StudentId,
                        principalTable: "users_user",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tutorship_attendances_users_user_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "users_user",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tutorship_interviews",
                columns: table => new
                {
                    InterviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    DateCompleted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tutorship_interviews", x => x.InterviewId);
                    table.ForeignKey(
                        name: "FK_tutorship_interviews_users_user_StudentId",
                        column: x => x.StudentId,
                        principalTable: "users_user",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tutorship_monitorings",
                columns: table => new
                {
                    MonitoringId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformanceLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DetailedObservations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionPlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tutorship_monitorings", x => x.MonitoringId);
                    table.ForeignKey(
                        name: "FK_tutorship_monitorings_users_user_StudentId",
                        column: x => x.StudentId,
                        principalTable: "users_user",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tutorship_monitorings_users_user_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "users_user",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tutorship_sessions",
                columns: table => new
                {
                    TutorshipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tutorship_sessions", x => x.TutorshipId);
                    table.ForeignKey(
                        name: "FK_tutorship_sessions_users_user_StudentId",
                        column: x => x.StudentId,
                        principalTable: "users_user",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tutorship_sessions_users_user_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "users_user",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users_auditlog",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_auditlog", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_users_auditlog_users_user_UserId",
                        column: x => x.UserId,
                        principalTable: "users_user",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_session",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_session", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_users_session_users_user_UserId",
                        column: x => x.UserId,
                        principalTable: "users_user",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_userrole",
                columns: table => new
                {
                    UserRoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_userrole", x => x.UserRoleId);
                    table.ForeignKey(
                        name: "FK_users_userrole_users_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "users_role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_userrole_users_user_UserId",
                        column: x => x.UserId,
                        principalTable: "users_user",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grades_grades",
                columns: table => new
                {
                    GradeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    SubjectUnitId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_grades", x => x.GradeId);
                    table.ForeignKey(
                        name: "FK_grades_grades_grades_group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "grades_group",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grades_grades_grades_subject_unit_SubjectUnitId",
                        column: x => x.SubjectUnitId,
                        principalTable: "grades_subject_unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grades_teacher_subject_group",
                columns: table => new
                {
                    TeacherSubjectGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherSubjectId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_teacher_subject_group", x => x.TeacherSubjectGroupId);
                    table.ForeignKey(
                        name: "FK_grades_teacher_subject_group_grades_group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "grades_group",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grades_teacher_subject_group_grades_teacher_subject_TeacherSubjectId",
                        column: x => x.TeacherSubjectId,
                        principalTable: "grades_teacher_subject",
                        principalColumn: "TeacherSubjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grades_extraordinary_grades",
                columns: table => new
                {
                    ExtraordinaryGradeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinalGradeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_extraordinary_grades", x => x.ExtraordinaryGradeId);
                    table.ForeignKey(
                        name: "FK_grades_extraordinary_grades_grades_final_grades_FinalGradeId",
                        column: x => x.FinalGradeId,
                        principalTable: "grades_final_grades",
                        principalColumn: "FinalGradeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "procedure_documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdProcedure = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedure_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_procedure_documents_procedure_request_IdProcedure",
                        column: x => x.IdProcedure,
                        principalTable: "procedure_request",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "procedure_monitoring",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdProcedure = table.Column<int>(type: "int", nullable: false),
                    IdStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedure_monitoring", x => x.Id);
                    table.ForeignKey(
                        name: "FK_procedure_monitoring_procedure_request_IdProcedure",
                        column: x => x.IdProcedure,
                        principalTable: "procedure_request",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_procedure_monitoring_procedure_status_IdStatus",
                        column: x => x.IdStatus,
                        principalTable: "procedure_status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tutorship_interview_answers",
                columns: table => new
                {
                    AnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterviewId = table.Column<int>(type: "int", nullable: false),
                    QuestionCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tutorship_interview_answers", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_tutorship_interview_answers_tutorship_interviews_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "tutorship_interviews",
                        principalColumn: "InterviewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grades_unit_recovery",
                columns: table => new
                {
                    UnitRecoveryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_unit_recovery", x => x.UnitRecoveryId);
                    table.ForeignKey(
                        name: "FK_grades_unit_recovery_grades_grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "grades_grades",
                        principalColumn: "GradeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_grades_extraordinary_grades_FinalGradeId",
                table: "grades_extraordinary_grades",
                column: "FinalGradeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grades_final_grades_GroupId",
                table: "grades_final_grades",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_final_grades_StudentId",
                table: "grades_final_grades",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_final_grades_StudentId_SubjectId_GroupId",
                table: "grades_final_grades",
                columns: new[] { "StudentId", "SubjectId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_grades_final_grades_SubjectId",
                table: "grades_final_grades",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_grades_GroupId",
                table: "grades_grades",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_grades_StudentId",
                table: "grades_grades",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_grades_StudentId_SubjectUnitId_GroupId",
                table: "grades_grades",
                columns: new[] { "StudentId", "SubjectUnitId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_grades_grades_SubjectUnitId",
                table: "grades_grades",
                column: "SubjectUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_group_GradeLevelId",
                table: "grades_group",
                column: "GradeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_group_SchoolCycleId",
                table: "grades_group",
                column: "SchoolCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_subject_unit_SubjectId_UnitNumber",
                table: "grades_subject_unit",
                columns: new[] { "SubjectId", "UnitNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grades_subjects_GradeLevelId",
                table: "grades_subjects",
                column: "GradeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_teacher_subject_SubjectId",
                table: "grades_teacher_subject",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_teacher_subject_group_GroupId",
                table: "grades_teacher_subject_group",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_teacher_subject_group_TeacherSubjectId_GroupId",
                table: "grades_teacher_subject_group",
                columns: new[] { "TeacherSubjectId", "GroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grades_unit_recovery_GradeId",
                table: "grades_unit_recovery",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_preenrollment_addresses_id_data",
                table: "preenrollment_addresses",
                column: "id_data");

            migrationBuilder.CreateIndex(
                name: "IX_preenrollment_general_preenrollment_careersIdCareer",
                table: "preenrollment_general",
                column: "preenrollment_careersIdCareer");

            migrationBuilder.CreateIndex(
                name: "IX_preenrollment_infos_id_data",
                table: "preenrollment_infos",
                column: "id_data");

            migrationBuilder.CreateIndex(
                name: "IX_preenrollment_schools_id_data",
                table: "preenrollment_schools",
                column: "id_data");

            migrationBuilder.CreateIndex(
                name: "IX_preenrollment_tutors_id_data",
                table: "preenrollment_tutors",
                column: "id_data");

            migrationBuilder.CreateIndex(
                name: "IX_procedure_documents_IdProcedure",
                table: "procedure_documents",
                column: "IdProcedure");

            migrationBuilder.CreateIndex(
                name: "IX_procedure_monitoring_IdProcedure",
                table: "procedure_monitoring",
                column: "IdProcedure");

            migrationBuilder.CreateIndex(
                name: "IX_procedure_monitoring_IdStatus",
                table: "procedure_monitoring",
                column: "IdStatus");

            migrationBuilder.CreateIndex(
                name: "IX_procedure_request_IdStatus",
                table: "procedure_request",
                column: "IdStatus");

            migrationBuilder.CreateIndex(
                name: "IX_procedure_request_IdTypeProcedure",
                table: "procedure_request",
                column: "IdTypeProcedure");

            migrationBuilder.CreateIndex(
                name: "IX_procedure_type_requirements_IdTypeDocument",
                table: "procedure_type_requirements",
                column: "IdTypeDocument");

            migrationBuilder.CreateIndex(
                name: "IX_procedure_type_requirements_IdTypeProcedure",
                table: "procedure_type_requirements",
                column: "IdTypeProcedure");

            migrationBuilder.CreateIndex(
                name: "IX_procedure_types_IdArea",
                table: "procedure_types",
                column: "IdArea");

            migrationBuilder.CreateIndex(
                name: "IX_tutorship_attendances_StudentId",
                table: "tutorship_attendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_tutorship_attendances_TeacherId",
                table: "tutorship_attendances",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_tutorship_interview_answers_InterviewId",
                table: "tutorship_interview_answers",
                column: "InterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_tutorship_interviews_StudentId",
                table: "tutorship_interviews",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_tutorship_monitorings_StudentId",
                table: "tutorship_monitorings",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_tutorship_monitorings_TeacherId",
                table: "tutorship_monitorings",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_tutorship_sessions_StudentId",
                table: "tutorship_sessions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_tutorship_sessions_TeacherId",
                table: "tutorship_sessions",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_users_auditlog_UserId",
                table: "users_auditlog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_rolepermission_PermissionId",
                table: "users_rolepermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_users_rolepermission_RoleId",
                table: "users_rolepermission",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_users_session_UserId",
                table: "users_session",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_user_PersonId",
                table: "users_user",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_userrole_RoleId",
                table: "users_userrole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_users_userrole_UserId",
                table: "users_userrole",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grades_extraordinary_grades");

            migrationBuilder.DropTable(
                name: "grades_teacher_subject_group");

            migrationBuilder.DropTable(
                name: "grades_unit_recovery");

            migrationBuilder.DropTable(
                name: "preenrollment_addresses");

            migrationBuilder.DropTable(
                name: "preenrollment_infos");

            migrationBuilder.DropTable(
                name: "preenrollment_schools");

            migrationBuilder.DropTable(
                name: "preenrollment_tutors");

            migrationBuilder.DropTable(
                name: "procedure_documents");

            migrationBuilder.DropTable(
                name: "procedure_monitoring");

            migrationBuilder.DropTable(
                name: "procedure_type_requirements");

            migrationBuilder.DropTable(
                name: "tutorship_attendances");

            migrationBuilder.DropTable(
                name: "tutorship_interview_answers");

            migrationBuilder.DropTable(
                name: "tutorship_monitorings");

            migrationBuilder.DropTable(
                name: "tutorship_sessions");

            migrationBuilder.DropTable(
                name: "users_auditlog");

            migrationBuilder.DropTable(
                name: "users_rolepermission");

            migrationBuilder.DropTable(
                name: "users_session");

            migrationBuilder.DropTable(
                name: "users_userrole");

            migrationBuilder.DropTable(
                name: "grades_final_grades");

            migrationBuilder.DropTable(
                name: "grades_teacher_subject");

            migrationBuilder.DropTable(
                name: "grades_grades");

            migrationBuilder.DropTable(
                name: "preenrollment_general");

            migrationBuilder.DropTable(
                name: "procedure_request");

            migrationBuilder.DropTable(
                name: "procedure_type_documents");

            migrationBuilder.DropTable(
                name: "tutorship_interviews");

            migrationBuilder.DropTable(
                name: "users_permission");

            migrationBuilder.DropTable(
                name: "users_role");

            migrationBuilder.DropTable(
                name: "grades_group");

            migrationBuilder.DropTable(
                name: "grades_subject_unit");

            migrationBuilder.DropTable(
                name: "preenrollment_careers");

            migrationBuilder.DropTable(
                name: "procedure_status");

            migrationBuilder.DropTable(
                name: "procedure_types");

            migrationBuilder.DropTable(
                name: "users_user");

            migrationBuilder.DropTable(
                name: "grades_school_cycle");

            migrationBuilder.DropTable(
                name: "grades_subjects");

            migrationBuilder.DropTable(
                name: "procedure_areas");

            migrationBuilder.DropTable(
                name: "users_person");

            migrationBuilder.DropTable(
                name: "grades_grade_level");
        }
    }
}
