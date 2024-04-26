using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace old_planner_api.Migrations
{
    /// <inheritdoc />
    public partial class change : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Boards_BoardId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_TaskChats_TaskId",
                table: "TaskChats");

            migrationBuilder.RenameColumn(
                name: "BoardId",
                table: "Tasks",
                newName: "ColumnId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_BoardId",
                table: "Tasks",
                newName: "IX_Tasks_ColumnId");

            migrationBuilder.AddColumn<Guid>(
                name: "ChatId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "BoardColumns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardColumns_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ChatId",
                table: "Tasks",
                column: "ChatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskChats_TaskId",
                table: "TaskChats",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardColumns_BoardId",
                table: "BoardColumns",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_BoardColumns_ColumnId",
                table: "Tasks",
                column: "ColumnId",
                principalTable: "BoardColumns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Chats_ChatId",
                table: "Tasks",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_BoardColumns_ColumnId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Chats_ChatId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "BoardColumns");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ChatId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_TaskChats_TaskId",
                table: "TaskChats");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "ColumnId",
                table: "Tasks",
                newName: "BoardId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_ColumnId",
                table: "Tasks",
                newName: "IX_Tasks_BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChats_TaskId",
                table: "TaskChats",
                column: "TaskId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Boards_BoardId",
                table: "Tasks",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
