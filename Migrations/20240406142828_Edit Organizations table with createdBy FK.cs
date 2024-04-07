using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class EditOrganizationstablewithcreatedByFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_AspNetUsers_CreatedBy",
                table: "Organizations");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Organizations",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Organizations_CreatedBy",
                table: "Organizations",
                newName: "IX_Organizations_CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_AspNetUsers_CreatedById",
                table: "Organizations",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_AspNetUsers_CreatedById",
                table: "Organizations");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "Organizations",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Organizations_CreatedById",
                table: "Organizations",
                newName: "IX_Organizations_CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_AspNetUsers_CreatedBy",
                table: "Organizations",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
