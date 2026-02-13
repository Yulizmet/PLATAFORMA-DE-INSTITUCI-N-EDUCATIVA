using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManager.Migrations
{
    /// <inheritdoc />
    public partial class Add_new_Grades_Entities : Migration
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

            //migrationBuilder.CreateTable(
            //    name: "procedure_areas",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_procedure_areas", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "procedure_status",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        BackgroundColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
            //        TextColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
            //        StepOrder = table.Column<int>(type: "int", nullable: false),
            //        DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_procedure_status", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "procedure_type_documents",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
            //        DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_procedure_type_documents", x => x.Id);
            //    });

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

            //migrationBuilder.CreateTable(
            //    name: "procedure_types",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
            //        DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        IdArea = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_procedure_types", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_procedure_types_procedure_areas_IdArea",
            //            column: x => x.IdArea,
            //            principalTable: "procedure_areas",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

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

            //migrationBuilder.CreateTable(
            //    name: "procedure_request",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Folio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            //        DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            //        Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IdUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IdTypeProcedure = table.Column<int>(type: "int", nullable: false),
            //        IdStatus = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_procedure_request", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_procedure_request_procedure_status_IdStatus",
            //            column: x => x.IdStatus,
            //            principalTable: "procedure_status",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_procedure_request_procedure_types_IdTypeProcedure",
            //            column: x => x.IdTypeProcedure,
            //            principalTable: "procedure_types",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "procedure_type_requirements",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        IdTypeProcedure = table.Column<int>(type: "int", nullable: false),
            //        IdTypeDocument = table.Column<int>(type: "int", nullable: false),
            //        IsRequired = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_procedure_type_requirements", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_procedure_type_requirements_procedure_type_documents_IdTypeDocument",
            //            column: x => x.IdTypeDocument,
            //            principalTable: "procedure_type_documents",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_procedure_type_requirements_procedure_types_IdTypeProcedure",
            //            column: x => x.IdTypeProcedure,
            //            principalTable: "procedure_types",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

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

            //migrationBuilder.CreateTable(
            //    name: "procedure_documents",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            //        FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        IdProcedure = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_procedure_documents", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_procedure_documents_procedure_request_IdProcedure",
            //            column: x => x.IdProcedure,
            //            principalTable: "procedure_request",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "procedure_monitoring",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        IdUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IdProcedure = table.Column<int>(type: "int", nullable: false),
            //        IdStatus = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_procedure_monitoring", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_procedure_monitoring_procedure_request_IdProcedure",
            //            column: x => x.IdProcedure,
            //            principalTable: "procedure_request",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_procedure_monitoring_procedure_status_IdStatus",
            //            column: x => x.IdStatus,
            //            principalTable: "procedure_status",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

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

            //migrationBuilder.CreateIndex(
            //    name: "IX_procedure_documents_IdProcedure",
            //    table: "procedure_documents",
            //    column: "IdProcedure");

            //migrationBuilder.CreateIndex(
            //    name: "IX_procedure_monitoring_IdProcedure",
            //    table: "procedure_monitoring",
            //    column: "IdProcedure");

            //migrationBuilder.CreateIndex(
            //    name: "IX_procedure_monitoring_IdStatus",
            //    table: "procedure_monitoring",
            //    column: "IdStatus");

            //migrationBuilder.CreateIndex(
            //    name: "IX_procedure_request_IdStatus",
            //    table: "procedure_request",
            //    column: "IdStatus");

            //migrationBuilder.CreateIndex(
            //    name: "IX_procedure_request_IdTypeProcedure",
            //    table: "procedure_request",
            //    column: "IdTypeProcedure");

            //migrationBuilder.CreateIndex(
            //    name: "IX_procedure_type_requirements_IdTypeDocument",
            //    table: "procedure_type_requirements",
            //    column: "IdTypeDocument");

            //migrationBuilder.CreateIndex(
            //    name: "IX_procedure_type_requirements_IdTypeProcedure",
            //    table: "procedure_type_requirements",
            //    column: "IdTypeProcedure");

            //migrationBuilder.CreateIndex(
            //    name: "IX_procedure_types_IdArea",
            //    table: "procedure_types",
            //    column: "IdArea");
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
                name: "procedure_documents");

            migrationBuilder.DropTable(
                name: "procedure_monitoring");

            migrationBuilder.DropTable(
                name: "procedure_type_requirements");

            migrationBuilder.DropTable(
                name: "grades_final_grades");

            migrationBuilder.DropTable(
                name: "grades_teacher_subject");

            migrationBuilder.DropTable(
                name: "grades_grades");

            migrationBuilder.DropTable(
                name: "procedure_request");

            migrationBuilder.DropTable(
                name: "procedure_type_documents");

            migrationBuilder.DropTable(
                name: "grades_group");

            migrationBuilder.DropTable(
                name: "grades_subject_unit");

            migrationBuilder.DropTable(
                name: "procedure_status");

            migrationBuilder.DropTable(
                name: "procedure_types");

            migrationBuilder.DropTable(
                name: "grades_school_cycle");

            migrationBuilder.DropTable(
                name: "grades_subjects");

            migrationBuilder.DropTable(
                name: "procedure_areas");

            migrationBuilder.DropTable(
                name: "grades_grade_level");
        }
    }
}
