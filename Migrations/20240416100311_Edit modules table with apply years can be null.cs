using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class Editmodulestablewithapplyyearscanbenull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_SchoolYears_ApplyYearId",
                table: "Modules");

            migrationBuilder.AlterColumn<int>(
                name: "ApplyYearId",
                table: "Modules",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_SchoolYears_ApplyYearId",
                table: "Modules",
                column: "ApplyYearId",
                principalTable: "SchoolYears",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_SchoolYears_ApplyYearId",
                table: "Modules");

            migrationBuilder.AlterColumn<int>(
                name: "ApplyYearId",
                table: "Modules",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_SchoolYears_ApplyYearId",
                table: "Modules",
                column: "ApplyYearId",
                principalTable: "SchoolYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
