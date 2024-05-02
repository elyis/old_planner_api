using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace old_planner_api.Migrations
{
    /// <inheritdoc />
    public partial class task_performer_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Chats_ChatId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ChatId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Tasks");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Tasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TaskId",
                table: "Chats",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BoardColumnTasks",
                columns: table => new
                {
                    ColumnId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardColumnTasks", x => new { x.TaskId, x.ColumnId });
                    table.ForeignKey(
                        name: "FK_BoardColumnTasks_BoardColumns_ColumnId",
                        column: x => x.ColumnId,
                        principalTable: "BoardColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoardColumnTasks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskPerformers",
                columns: table => new
                {
                    PerformerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskPerformers", x => new { x.PerformerId, x.TaskId });
                    table.ForeignKey(
                        name: "FK_TaskPerformers_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskPerformers_Users_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_TaskId",
                table: "Chats",
                column: "TaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumnTasks_ColumnId",
                table: "BoardColumnTasks",
                column: "ColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskPerformers_TaskId",
                table: "TaskPerformers",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Tasks_TaskId",
                table: "Chats",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Tasks_TaskId",
                table: "Chats");

            migrationBuilder.DropTable(
                name: "BoardColumnTasks");

            migrationBuilder.DropTable(
                name: "TaskPerformers");

            migrationBuilder.DropIndex(
                name: "IX_Chats_TaskId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "Chats");

            migrationBuilder.AddColumn<Guid>(
                name: "ChatId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ChatId",
                table: "Tasks",
                column: "ChatId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Chats_ChatId",
                table: "Tasks",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
