using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class AddUSmartTokenstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2a62db4d-08ae-4b20-8b53-65eb22d65e69", "65d4bd87-c5eb-4611-9dc0-1f4d6f7d2d8a" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2a62db4d-08ae-4b20-8b53-65eb22d65e69");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "65d4bd87-c5eb-4611-9dc0-1f4d6f7d2d8a");

            migrationBuilder.CreateTable(
                name: "USmartTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsExpired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USmartTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_USmartTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USmartTokens_UserId",
                table: "USmartTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "USmartTokens");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "2a62db4d-08ae-4b20-8b53-65eb22d65e69", null, "Quản trị viên", "QUẢN TRỊ VIÊN" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "Avatar", "ConcurrencyStamp", "CreatedAt", "DateOfBirth", "Email", "EmailConfirmed", "FirstName", "Gender", "IsDeleted", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "USmartId", "UserName" },
                values: new object[] { "65d4bd87-c5eb-4611-9dc0-1f4d6f7d2d8a", 0, "Vinh, Nghệ An", null, "ed84669e-e560-4251-bbfc-ebcb71344ace", new DateTime(2024, 3, 9, 17, 59, 24, 435, DateTimeKind.Utc).AddTicks(2790), new DateOnly(2002, 7, 2), "admin@admin.com", true, "Nguyễn Ngọc Anh", 1, null, "Tuấn", false, null, "ADMIN@ADMIN.COM", "ADMIN", "AQAAAAIAAYagAAAAEMUskR/QNBtZxI1/Fj5wsqMlrwpTXT5r/cO10WV6d94aIZ75NjaNkonrlzlg1W4sPg==", "0123456789", false, "bb0f2a7a-82c9-46e6-8e10-f36942d5134b", false, null, "admin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "2a62db4d-08ae-4b20-8b53-65eb22d65e69", "65d4bd87-c5eb-4611-9dc0-1f4d6f7d2d8a" });
        }
    }
}
