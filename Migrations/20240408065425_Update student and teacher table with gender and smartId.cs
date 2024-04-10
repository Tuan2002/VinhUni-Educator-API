using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatestudentandteachertablewithgenderandsmartId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SmartId",
                table: "Teachers",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Gender",
                table: "Students",
                type: "integer USING \"Gender\"::integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmartId",
                table: "Students",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmartId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "SmartId",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Students",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
