using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace old_planner_api.Migrations
{
    /// <inheritdoc />
    public partial class join_chat_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskChatMessages");

            migrationBuilder.DropTable(
                name: "UserTaskChatSessions");

            migrationBuilder.DropTable(
                name: "TaskChatMemberships");

            migrationBuilder.DropTable(
                name: "TaskChats");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskChats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskChats_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskChatMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateLastViewing = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskChatMemberships", x => x.Id);
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "UserTaskChatSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateLastViewing = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTaskChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTaskChatSessions_TaskChatMemberships_ChatMembershipId",
                        column: x => x.ChatMembershipId,
                        principalTable: "TaskChatMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTaskChatSessions_UserSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskChatMemberships_ChatId",
                table: "TaskChatMemberships",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChatMemberships_ParticipantId",
                table: "TaskChatMemberships",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChatMessages_ChatId",
                table: "TaskChatMessages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChatMessages_SenderId",
                table: "TaskChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChats_TaskId",
                table: "TaskChats",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTaskChatSessions_ChatMembershipId",
                table: "UserTaskChatSessions",
                column: "ChatMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTaskChatSessions_SessionId",
                table: "UserTaskChatSessions",
                column: "SessionId");
        }
    }
}
