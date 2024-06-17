using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class AddClassmodulestable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleClasses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ModuleClassId = table.Column<int>(type: "integer", nullable: false),
                    ModuleClassCode = table.Column<string>(type: "text", nullable: false),
                    ModuleClassName = table.Column<string>(type: "text", nullable: false),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    SemesterId = table.Column<int>(type: "integer", nullable: false),
                    IsChildClass = table.Column<bool>(type: "boolean", nullable: false),
                    ParentClassId = table.Column<string>(type: "text", nullable: true),
                    MaxStudents = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleClasses_ModuleClasses_ParentClassId",
                        column: x => x.ParentClassId,
                        principalTable: "ModuleClasses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ModuleClasses_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleClasses_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleClasses_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleClasses_ModuleId",
                table: "ModuleClasses",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleClasses_ParentClassId",
                table: "ModuleClasses",
                column: "ParentClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleClasses_SemesterId",
                table: "ModuleClasses",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleClasses_TeacherId",
                table: "ModuleClasses",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleClasses");
        }
    }
}
