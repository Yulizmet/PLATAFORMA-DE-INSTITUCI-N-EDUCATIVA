using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManager.Migrations
{
    /// <inheritdoc />
    public partial class UsersEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.CreateTable(
                name: "users_permission",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_role", x => x.RoleId);
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
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                name: "users_auditlog");

            migrationBuilder.DropTable(
                name: "users_rolepermission");

            migrationBuilder.DropTable(
                name: "users_session");

            migrationBuilder.DropTable(
                name: "users_userrole");

            migrationBuilder.DropTable(
                name: "preenrollment_general");

            migrationBuilder.DropTable(
                name: "procedure_request");

            migrationBuilder.DropTable(
                name: "procedure_type_documents");

            migrationBuilder.DropTable(
                name: "users_permission");

            migrationBuilder.DropTable(
                name: "users_role");

            migrationBuilder.DropTable(
                name: "users_user");

            migrationBuilder.DropTable(
                name: "preenrollment_careers");

            migrationBuilder.DropTable(
                name: "procedure_status");

            migrationBuilder.DropTable(
                name: "procedure_types");

            migrationBuilder.DropTable(
                name: "users_person");

            migrationBuilder.DropTable(
                name: "procedure_areas");
        }
    }
}
