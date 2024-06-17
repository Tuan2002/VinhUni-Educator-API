using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleClassStudentstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleClassStudents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ModuleClassId = table.Column<string>(type: "text", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    SemesterId = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AddedById = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleClassStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleClassStudents_AspNetUsers_AddedById",
                        column: x => x.AddedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleClassStudents_ModuleClasses_ModuleClassId",
                        column: x => x.ModuleClassId,
                        principalTable: "ModuleClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleClassStudents_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleClassStudents_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleClassStudents_AddedById",
                table: "ModuleClassStudents",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleClassStudents_ModuleClassId",
                table: "ModuleClassStudents",
                column: "ModuleClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleClassStudents_SemesterId",
                table: "ModuleClassStudents",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleClassStudents_StudentId",
                table: "ModuleClassStudents",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleClassStudents");
        }
    }
}
