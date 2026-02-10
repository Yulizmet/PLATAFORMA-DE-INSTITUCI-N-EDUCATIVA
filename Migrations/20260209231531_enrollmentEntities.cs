using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManager.Migrations
{
    /// <inheritdoc />
    public partial class enrollmentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "preenrollment_general",
                columns: table => new
                {
                    IdData = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCareer = table.Column<int>(type: "int", nullable: false),
                    PaternalLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaternalLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Curp = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preenrollment_general", x => x.IdData);
                    table.ForeignKey(
                        name: "FK_preenrollment_general_preenrollment_careers_IdCareer",
                        column: x => x.IdCareer,
                        principalTable: "preenrollment_careers",
                        principalColumn: "IdCareer",
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

            migrationBuilder.CreateIndex(
                name: "IX_preenrollment_addresses_id_data",
                table: "preenrollment_addresses",
                column: "id_data");

            migrationBuilder.CreateIndex(
                name: "IX_preenrollment_general_Curp",
                table: "preenrollment_general",
                column: "Curp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preenrollment_general_Email",
                table: "preenrollment_general",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preenrollment_general_IdCareer",
                table: "preenrollment_general",
                column: "IdCareer");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "preenrollment_addresses");

            migrationBuilder.DropTable(
                name: "preenrollment_infos");

            migrationBuilder.DropTable(
                name: "preenrollment_schools");

            migrationBuilder.DropTable(
                name: "preenrollment_tutors");

            migrationBuilder.DropTable(
                name: "preenrollment_general");

            migrationBuilder.DropTable(
                name: "preenrollment_careers");
        }
    }
}
