using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMajorstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Majors_AspNetUsers_CreatorId",
                table: "Majors");

            migrationBuilder.DropForeignKey(
                name: "FK_Majors_AspNetUsers_DeletedId",
                table: "Majors");

            migrationBuilder.DropIndex(
                name: "IX_Majors_CreatorId",
                table: "Majors");

            migrationBuilder.DropIndex(
                name: "IX_Majors_DeletedId",
                table: "Majors");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Majors");

            migrationBuilder.DropColumn(
                name: "DeletedId",
                table: "Majors");

            migrationBuilder.RenameColumn(
                name: "DeletedById",
                table: "Majors",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "Majors",
                newName: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Majors_CreatedBy",
                table: "Majors",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Majors_AspNetUsers_CreatedBy",
                table: "Majors",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Majors_AspNetUsers_CreatedBy",
                table: "Majors");

            migrationBuilder.DropIndex(
                name: "IX_Majors_CreatedBy",
                table: "Majors");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Majors",
                newName: "DeletedById");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Majors",
                newName: "CreatedById");

            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Majors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedId",
                table: "Majors",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Majors_CreatorId",
                table: "Majors",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Majors_DeletedId",
                table: "Majors",
                column: "DeletedId");

            migrationBuilder.AddForeignKey(
                name: "FK_Majors_AspNetUsers_CreatorId",
                table: "Majors",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Majors_AspNetUsers_DeletedId",
                table: "Majors",
                column: "DeletedId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
