using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
