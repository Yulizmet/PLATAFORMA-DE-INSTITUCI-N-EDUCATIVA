using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManager.Migrations
{
    /// <inheritdoc />
    public partial class ProceduresEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcedureAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false)
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
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdArea = table.Column<int>(type: "int", nullable: false)
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
                    Folio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdTypeProcedure = table.Column<int>(type: "int", nullable: false),
                    IdStatus = table.Column<int>(type: "int", nullable: false)
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
                    IdTypeProcedure = table.Column<int>(type: "int", nullable: false),
                    IdTypeDocument = table.Column<int>(type: "int", nullable: false),
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
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdProcedure = table.Column<int>(type: "int", nullable: false)
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
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdProcedure = table.Column<int>(type: "int", nullable: false),
                    IdStatus = table.Column<int>(type: "int", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
