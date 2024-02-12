using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace old_planner_api.Migrations
{
    /// <inheritdoc />
    public partial class renaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_TaskChats_ChatId",
                table: "ChatMessages");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "UserChatHistories");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Chats");

            migrationBuilder.RenameColumn(
                name: "CreatedAtDate",
                table: "ChatMessages",
                newName: "SentAt");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Chats",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TaskChatMemberships",
                columns: table => new
                {
                    ChatId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DateLastViewing = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskChatMemberships", x => new { x.ParticipantId, x.ChatId });
                    table.ForeignKey(
                        name: "FK_TaskChatMemberships_TaskChats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "TaskChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskChatMemberships_Users_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SenderId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskChatMessages_TaskChats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "TaskChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskChatMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskChatMemberships_ChatId",
                table: "TaskChatMemberships",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChatMessages_ChatId",
                table: "TaskChatMessages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChatMessages_SenderId",
                table: "TaskChatMessages",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Chats_ChatId",
                table: "ChatMessages",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Chats_ChatId",
                table: "ChatMessages");

            migrationBuilder.DropTable(
                name: "TaskChatMemberships");

            migrationBuilder.DropTable(
                name: "TaskChatMessages");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Chats");

            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "ChatMessages",
                newName: "CreatedAtDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Chats",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChatId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SenderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserChatHistories",
                columns: table => new
                {
                    ParticipantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChatId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DateLastViewing = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChatHistories", x => new { x.ParticipantId, x.ChatId });
                    table.ForeignKey(
                        name: "FK_UserChatHistories_TaskChats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "TaskChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChatHistories_Users_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId",
                table: "Messages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChatHistories_ChatId",
                table: "UserChatHistories",
                column: "ChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_TaskChats_ChatId",
                table: "ChatMessages",
                column: "ChatId",
                principalTable: "TaskChats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
