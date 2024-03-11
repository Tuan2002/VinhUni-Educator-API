using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class ChangerefreshTokenstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "9bac8314-e420-4b07-9322-5ed20415845e", "0ca9df2e-11be-4a25-910b-c5193c9e750a" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9bac8314-e420-4b07-9322-5ed20415845e");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "0ca9df2e-11be-4a25-910b-c5193c9e750a");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RefreshTokens");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "JwtId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

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

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RefreshTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "9bac8314-e420-4b07-9322-5ed20415845e", null, "Quản trị viên", "QUẢN TRỊ VIÊN" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "Avatar", "ConcurrencyStamp", "CreatedAt", "DateOfBirth", "Email", "EmailConfirmed", "FirstName", "Gender", "IsDeleted", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "USmartId", "UserName" },
                values: new object[] { "0ca9df2e-11be-4a25-910b-c5193c9e750a", 0, "Vinh, Nghệ An", null, "c3162936-bc75-4267-a1df-87081031eadd", new DateTime(2024, 2, 29, 14, 51, 59, 392, DateTimeKind.Utc).AddTicks(1620), new DateOnly(2002, 7, 2), "admin@admin.com", true, "Nguyễn Ngọc Anh", 1, null, "Tuấn", false, null, "ADMIN@ADMIN.COM", "ADMIN", "AQAAAAIAAYagAAAAELaGwJv55Qa4HEE6x6j3EPjKC1IVLDzXqyuMnNoMpkTtOsL2N8yPInypcW7EUhvICw==", "0123456789", false, "a98adf14-57ce-42ec-b043-66509d57b64f", false, null, "admin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "9bac8314-e420-4b07-9322-5ed20415845e", "0ca9df2e-11be-4a25-910b-c5193c9e750a" });
        }
    }
}
