using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class Addexamsseasontables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamSeasons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SeasonCode = table.Column<string>(type: "text", nullable: false),
                    SeasonName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationInMinutes = table.Column<int>(type: "integer", nullable: false),
                    UsePassword = table.Column<bool>(type: "boolean", nullable: false),
                    AllowRetry = table.Column<bool>(type: "boolean", nullable: false),
                    MaxRetryTurn = table.Column<int>(type: "integer", nullable: false),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false),
                    ShowResult = table.Column<bool>(type: "boolean", nullable: false),
                    ShowPoint = table.Column<bool>(type: "boolean", nullable: false),
                    SemesterId = table.Column<int>(type: "integer", nullable: false),
                    ExamId = table.Column<string>(type: "text", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSeasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSeasons_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSeasons_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSeasons_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSeasons_Teachers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamAssignedClasses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ExamSeasonId = table.Column<string>(type: "text", nullable: false),
                    ModuleClassId = table.Column<string>(type: "text", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamAssignedClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamAssignedClasses_ExamSeasons_ExamSeasonId",
                        column: x => x.ExamSeasonId,
                        principalTable: "ExamSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamAssignedClasses_ModuleClasses_ModuleClassId",
                        column: x => x.ModuleClassId,
                        principalTable: "ModuleClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamParticipants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ExamSeasonId = table.Column<string>(type: "text", nullable: false),
                    AssignedClassId = table.Column<string>(type: "text", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamParticipants_ExamAssignedClasses_AssignedClassId",
                        column: x => x.AssignedClassId,
                        principalTable: "ExamAssignedClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamParticipants_ExamSeasons_ExamSeasonId",
                        column: x => x.ExamSeasonId,
                        principalTable: "ExamSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamParticipants_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamTurns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ExamSeasonId = table.Column<string>(type: "text", nullable: false),
                    ExamParticipantId = table.Column<string>(type: "text", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamTurns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamTurns_ExamParticipants_ExamParticipantId",
                        column: x => x.ExamParticipantId,
                        principalTable: "ExamParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamTurns_ExamSeasons_ExamSeasonId",
                        column: x => x.ExamSeasonId,
                        principalTable: "ExamSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ExamTurnId = table.Column<string>(type: "text", nullable: false),
                    TotalPoint = table.Column<decimal>(type: "numeric", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamResults_ExamTurns_ExamTurnId",
                        column: x => x.ExamTurnId,
                        principalTable: "ExamTurns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamResultDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ExamResultId = table.Column<string>(type: "text", nullable: false),
                    QuestionId = table.Column<string>(type: "text", nullable: false),
                    SelectedAnswerId = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResultDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamResultDetails_ExamResults_ExamResultId",
                        column: x => x.ExamResultId,
                        principalTable: "ExamResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResultDetails_QuestionAnswers_SelectedAnswerId",
                        column: x => x.SelectedAnswerId,
                        principalTable: "QuestionAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResultDetails_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamAssignedClasses_ExamSeasonId",
                table: "ExamAssignedClasses",
                column: "ExamSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamAssignedClasses_ModuleClassId",
                table: "ExamAssignedClasses",
                column: "ModuleClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamParticipants_AssignedClassId",
                table: "ExamParticipants",
                column: "AssignedClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamParticipants_ExamSeasonId",
                table: "ExamParticipants",
                column: "ExamSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamParticipants_StudentId",
                table: "ExamParticipants",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResultDetails_ExamResultId",
                table: "ExamResultDetails",
                column: "ExamResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResultDetails_QuestionId",
                table: "ExamResultDetails",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResultDetails_SelectedAnswerId",
                table: "ExamResultDetails",
                column: "SelectedAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_ExamTurnId",
                table: "ExamResults",
                column: "ExamTurnId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSeasons_CreatedById",
                table: "ExamSeasons",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSeasons_ExamId",
                table: "ExamSeasons",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSeasons_OwnerId",
                table: "ExamSeasons",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSeasons_SemesterId",
                table: "ExamSeasons",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamTurns_ExamParticipantId",
                table: "ExamTurns",
                column: "ExamParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamTurns_ExamSeasonId",
                table: "ExamTurns",
                column: "ExamSeasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamResultDetails");

            migrationBuilder.DropTable(
                name: "ExamResults");

            migrationBuilder.DropTable(
                name: "ExamTurns");

            migrationBuilder.DropTable(
                name: "ExamParticipants");

            migrationBuilder.DropTable(
                name: "ExamAssignedClasses");

            migrationBuilder.DropTable(
                name: "ExamSeasons");
        }
    }
}
