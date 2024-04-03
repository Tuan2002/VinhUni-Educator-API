using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class EditMajorstablewithcreatedById : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_ApplicationUserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Majors_AspNetUsers_CreatedBy",
                table: "Majors");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_ApplicationUserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "AspNetUserRoles");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Majors",
                newName: "CreatedById");

            migrationBuilder.RenameIndex(
                name: "IX_Majors_CreatedBy",
                table: "Majors",
                newName: "IX_Majors_CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Majors_AspNetUsers_CreatedById",
                table: "Majors",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Majors_AspNetUsers_CreatedById",
                table: "Majors");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "Majors",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Majors_CreatedById",
                table: "Majors",
                newName: "IX_Majors_CreatedBy");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "AspNetUserRoles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_ApplicationUserId",
                table: "AspNetUserRoles",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_ApplicationUserId",
                table: "AspNetUserRoles",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Majors_AspNetUsers_CreatedBy",
                table: "Majors",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
