using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace old_planner_api.Migrations
{
    /// <inheritdoc />
    public partial class auth_provider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Token",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TokenValidBefore",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "UserSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenValidBefore",
                table: "UserSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizationProvider",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "TokenValidBefore",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "AuthorizationProvider",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenValidBefore",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Token",
                table: "Users",
                column: "Token");
        }
    }
}
