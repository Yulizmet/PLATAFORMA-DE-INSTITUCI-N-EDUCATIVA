using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManager.Migrations
{
    /// <inheritdoc />
    public partial class Add_Grades_Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcedureDocuments");

            migrationBuilder.DropTable(
                name: "ProcedureMonitoring");

            migrationBuilder.DropTable(
                name: "ProcedureTypeRequirements");

            migrationBuilder.DropTable(
                name: "ProcedureRequest");

            migrationBuilder.DropTable(
                name: "ProcedureTypeDocuments");

            migrationBuilder.DropTable(
                name: "ProcedureStatus");

            migrationBuilder.DropTable(
                name: "ProcedureTypes");

            migrationBuilder.DropTable(
                name: "ProcedureAreas");

            migrationBuilder.CreateTable(
                name: "grades_GradeLevels",
                columns: table => new
                {
                    GradeLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_GradeLevels", x => x.GradeLevelId);
                });

            migrationBuilder.CreateTable(
                name: "grades_SchoolCycles",
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
                    table.PrimaryKey("PK_grades_SchoolCycles", x => x.SchoolCycleId);
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
                name: "grades_Subjects",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GradeLevelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_Subjects", x => x.SubjectId);
                    table.ForeignKey(
                        name: "FK_grades_Subjects_grades_GradeLevels_GradeLevelId",
                        column: x => x.GradeLevelId,
                        principalTable: "grades_GradeLevels",
                        principalColumn: "GradeLevelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grades_GradeGroups",
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
                    table.PrimaryKey("PK_grades_GradeGroups", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_grades_GradeGroups_grades_GradeLevels_GradeLevelId",
                        column: x => x.GradeLevelId,
                        principalTable: "grades_GradeLevels",
                        principalColumn: "GradeLevelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grades_GradeGroups_grades_SchoolCycles_SchoolCycleId",
                        column: x => x.SchoolCycleId,
                        principalTable: "grades_SchoolCycles",
                        principalColumn: "SchoolCycleId",
                        onDelete: ReferentialAction.Restrict);
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
                name: "grades_SubjectUnits",
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
                    table.PrimaryKey("PK_grades_SubjectUnits", x => x.UnitId);
                    table.ForeignKey(
                        name: "FK_grades_SubjectUnits_grades_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "grades_Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grades_TeacherSubjects",
                columns: table => new
                {
                    TeacherSubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_TeacherSubjects", x => x.TeacherSubjectId);
                    table.ForeignKey(
                        name: "FK_grades_TeacherSubjects_grades_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "grades_Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grades_FinalGrades",
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
                    table.PrimaryKey("PK_grades_FinalGrades", x => x.FinalGradeId);
                    table.ForeignKey(
                        name: "FK_grades_FinalGrades_grades_GradeGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "grades_GradeGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grades_FinalGrades_grades_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "grades_Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Restrict);
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
                name: "grades_Grades",
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
                    table.PrimaryKey("PK_grades_Grades", x => x.GradeId);
                    table.ForeignKey(
                        name: "FK_grades_Grades_grades_GradeGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "grades_GradeGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grades_Grades_grades_SubjectUnits_SubjectUnitId",
                        column: x => x.SubjectUnitId,
                        principalTable: "grades_SubjectUnits",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grades_TeacherSubjectGroups",
                columns: table => new
                {
                    TeacherSubjectGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherSubjectId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grades_TeacherSubjectGroups", x => x.TeacherSubjectGroupId);
                    table.ForeignKey(
                        name: "FK_grades_TeacherSubjectGroups_grades_GradeGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "grades_GradeGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grades_TeacherSubjectGroups_grades_TeacherSubjects_TeacherSubjectId",
                        column: x => x.TeacherSubjectId,
                        principalTable: "grades_TeacherSubjects",
                        principalColumn: "TeacherSubjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grades_ExtraordinaryGrades",
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
                    table.PrimaryKey("PK_grades_ExtraordinaryGrades", x => x.ExtraordinaryGradeId);
                    table.ForeignKey(
                        name: "FK_grades_ExtraordinaryGrades_grades_FinalGrades_FinalGradeId",
                        column: x => x.FinalGradeId,
                        principalTable: "grades_FinalGrades",
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
                name: "grades_UnitRecoveries",
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
                    table.PrimaryKey("PK_grades_UnitRecoveries", x => x.UnitRecoveryId);
                    table.ForeignKey(
                        name: "FK_grades_UnitRecoveries_grades_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "grades_Grades",
                        principalColumn: "GradeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_grades_ExtraordinaryGrades_FinalGradeId",
                table: "grades_ExtraordinaryGrades",
                column: "FinalGradeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grades_FinalGrades_GroupId",
                table: "grades_FinalGrades",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_FinalGrades_StudentId",
                table: "grades_FinalGrades",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_FinalGrades_StudentId_SubjectId_GroupId",
                table: "grades_FinalGrades",
                columns: new[] { "StudentId", "SubjectId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_grades_FinalGrades_SubjectId",
                table: "grades_FinalGrades",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_GradeGroups_GradeLevelId",
                table: "grades_GradeGroups",
                column: "GradeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_GradeGroups_SchoolCycleId",
                table: "grades_GradeGroups",
                column: "SchoolCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_Grades_GroupId",
                table: "grades_Grades",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_Grades_StudentId",
                table: "grades_Grades",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_Grades_StudentId_SubjectUnitId_GroupId",
                table: "grades_Grades",
                columns: new[] { "StudentId", "SubjectUnitId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_grades_Grades_SubjectUnitId",
                table: "grades_Grades",
                column: "SubjectUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_Subjects_GradeLevelId",
                table: "grades_Subjects",
                column: "GradeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_SubjectUnits_SubjectId_UnitNumber",
                table: "grades_SubjectUnits",
                columns: new[] { "SubjectId", "UnitNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grades_TeacherSubjectGroups_GroupId",
                table: "grades_TeacherSubjectGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_TeacherSubjectGroups_TeacherSubjectId_GroupId",
                table: "grades_TeacherSubjectGroups",
                columns: new[] { "TeacherSubjectId", "GroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grades_TeacherSubjects_SubjectId",
                table: "grades_TeacherSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_UnitRecoveries_GradeId",
                table: "grades_UnitRecoveries",
                column: "GradeId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grades_ExtraordinaryGrades");

            migrationBuilder.DropTable(
                name: "grades_TeacherSubjectGroups");

            migrationBuilder.DropTable(
                name: "grades_UnitRecoveries");

            migrationBuilder.DropTable(
                name: "procedure_documents");

            migrationBuilder.DropTable(
                name: "procedure_monitoring");

            migrationBuilder.DropTable(
                name: "procedure_type_requirements");

            migrationBuilder.DropTable(
                name: "grades_FinalGrades");

            migrationBuilder.DropTable(
                name: "grades_TeacherSubjects");

            migrationBuilder.DropTable(
                name: "grades_Grades");

            migrationBuilder.DropTable(
                name: "procedure_request");

            migrationBuilder.DropTable(
                name: "procedure_type_documents");

            migrationBuilder.DropTable(
                name: "grades_GradeGroups");

            migrationBuilder.DropTable(
                name: "grades_SubjectUnits");

            migrationBuilder.DropTable(
                name: "procedure_status");

            migrationBuilder.DropTable(
                name: "procedure_types");

            migrationBuilder.DropTable(
                name: "grades_SchoolCycles");

            migrationBuilder.DropTable(
                name: "grades_Subjects");

            migrationBuilder.DropTable(
                name: "procedure_areas");

            migrationBuilder.DropTable(
                name: "grades_GradeLevels");

            migrationBuilder.CreateTable(
                name: "ProcedureAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcedureStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcedureTypeDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureTypeDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcedureTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdArea = table.Column<int>(type: "int", nullable: false),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcedureTypes_ProcedureAreas_IdArea",
                        column: x => x.IdArea,
                        principalTable: "ProcedureAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcedureRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdStatus = table.Column<int>(type: "int", nullable: false),
                    IdTypeProcedure = table.Column<int>(type: "int", nullable: false),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcedureRequest_ProcedureStatus_IdStatus",
                        column: x => x.IdStatus,
                        principalTable: "ProcedureStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcedureRequest_ProcedureTypes_IdTypeProcedure",
                        column: x => x.IdTypeProcedure,
                        principalTable: "ProcedureTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcedureTypeRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTypeDocument = table.Column<int>(type: "int", nullable: false),
                    IdTypeProcedure = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureTypeRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcedureTypeRequirements_ProcedureTypeDocuments_IdTypeDocument",
                        column: x => x.IdTypeDocument,
                        principalTable: "ProcedureTypeDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcedureTypeRequirements_ProcedureTypes_IdTypeProcedure",
                        column: x => x.IdTypeProcedure,
                        principalTable: "ProcedureTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcedureDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProcedure = table.Column<int>(type: "int", nullable: false),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcedureDocuments_ProcedureRequest_IdProcedure",
                        column: x => x.IdProcedure,
                        principalTable: "ProcedureRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcedureMonitoring",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProcedure = table.Column<int>(type: "int", nullable: false),
                    IdStatus = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUser = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureMonitoring", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcedureMonitoring_ProcedureRequest_IdProcedure",
                        column: x => x.IdProcedure,
                        principalTable: "ProcedureRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcedureMonitoring_ProcedureStatus_IdStatus",
                        column: x => x.IdStatus,
                        principalTable: "ProcedureStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureDocuments_IdProcedure",
                table: "ProcedureDocuments",
                column: "IdProcedure");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureMonitoring_IdProcedure",
                table: "ProcedureMonitoring",
                column: "IdProcedure");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureMonitoring_IdStatus",
                table: "ProcedureMonitoring",
                column: "IdStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureRequest_IdStatus",
                table: "ProcedureRequest",
                column: "IdStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureRequest_IdTypeProcedure",
                table: "ProcedureRequest",
                column: "IdTypeProcedure");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureTypeRequirements_IdTypeDocument",
                table: "ProcedureTypeRequirements",
                column: "IdTypeDocument");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureTypeRequirements_IdTypeProcedure",
                table: "ProcedureTypeRequirements",
                column: "IdTypeProcedure");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureTypes_IdArea",
                table: "ProcedureTypes",
                column: "IdArea");
        }
    }
}
