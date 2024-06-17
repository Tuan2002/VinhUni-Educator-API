using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class EditTraningProgramstablewithcreatedById : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPrograms_AspNetUsers_CreatedBy",
                table: "TrainingPrograms");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "TrainingPrograms",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingPrograms_CreatedBy",
                table: "TrainingPrograms",
                newName: "IX_TrainingPrograms_CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPrograms_AspNetUsers_CreatedById",
                table: "TrainingPrograms",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPrograms_AspNetUsers_CreatedById",
                table: "TrainingPrograms");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "TrainingPrograms",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingPrograms_CreatedById",
                table: "TrainingPrograms",
                newName: "IX_TrainingPrograms_CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPrograms_AspNetUsers_CreatedBy",
                table: "TrainingPrograms",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
