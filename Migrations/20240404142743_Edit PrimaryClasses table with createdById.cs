using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class EditPrimaryClassestablewithcreatedById : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrimaryClasses_AspNetUsers_CreatedBy",
                table: "PrimaryClasses");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "PrimaryClasses",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_PrimaryClasses_CreatedBy",
                table: "PrimaryClasses",
                newName: "IX_PrimaryClasses_CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_PrimaryClasses_AspNetUsers_CreatedById",
                table: "PrimaryClasses",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrimaryClasses_AspNetUsers_CreatedById",
                table: "PrimaryClasses");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "PrimaryClasses",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_PrimaryClasses_CreatedById",
                table: "PrimaryClasses",
                newName: "IX_PrimaryClasses_CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_PrimaryClasses_AspNetUsers_CreatedBy",
                table: "PrimaryClasses",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
