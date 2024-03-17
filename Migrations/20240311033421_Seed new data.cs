using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhUni_Educator_API.Migrations
{
    /// <inheritdoc />
    public partial class Seednewdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "e66cb6f0-9a39-4c93-9eb1-24a1381c4964", null, "Quản trị viên", "QUẢN TRỊ VIÊN" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "Avatar", "ConcurrencyStamp", "CreatedAt", "DateOfBirth", "Email", "EmailConfirmed", "FirstName", "Gender", "IsDeleted", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "USmartId", "UserName" },
                values: new object[] { "fb72c472-84df-4ce1-9db1-10fb7340b25e", 0, "Vinh, Nghệ An", null, "8a3d0359-07c5-482b-98c5-f906b1268464", new DateTime(2024, 3, 11, 3, 34, 21, 256, DateTimeKind.Utc).AddTicks(6560), new DateOnly(2002, 7, 2), "admin@admin.com", true, "Nguyễn Ngọc Anh", 1, null, "Tuấn", false, null, "ADMIN@ADMIN.COM", "ADMIN", "AQAAAAIAAYagAAAAEP/xcswT8ruU1spMFTOie+YnBvQsSt3kBHusPNPCZM3udN41Tu0xdP+M/Fa32xgOmw==", "0123456789", false, "df5b0f9a-2992-462c-9e67-5ced5b3df629", false, 78592, "admin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "e66cb6f0-9a39-4c93-9eb1-24a1381c4964", "fb72c472-84df-4ce1-9db1-10fb7340b25e" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "e66cb6f0-9a39-4c93-9eb1-24a1381c4964", "fb72c472-84df-4ce1-9db1-10fb7340b25e" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e66cb6f0-9a39-4c93-9eb1-24a1381c4964");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "fb72c472-84df-4ce1-9db1-10fb7340b25e");
        }
    }
}
