using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class Editexamresultdetailtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResultDetails_QuestionAnswers_SelectedAnswerId",
                table: "ExamResultDetails");

            migrationBuilder.AlterColumn<string>(
                name: "SelectedAnswerId",
                table: "ExamResultDetails",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResultDetails_QuestionAnswers_SelectedAnswerId",
                table: "ExamResultDetails",
                column: "SelectedAnswerId",
                principalTable: "QuestionAnswers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamResultDetails_QuestionAnswers_SelectedAnswerId",
                table: "ExamResultDetails");

            migrationBuilder.AlterColumn<string>(
                name: "SelectedAnswerId",
                table: "ExamResultDetails",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResultDetails_QuestionAnswers_SelectedAnswerId",
                table: "ExamResultDetails",
                column: "SelectedAnswerId",
                principalTable: "QuestionAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
