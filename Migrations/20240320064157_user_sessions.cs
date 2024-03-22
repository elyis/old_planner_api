using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace old_planner_api.Migrations
{
    /// <inheritdoc />
    public partial class user_sessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskChatMemberships",
                table: "TaskChatMemberships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatMemberships",
                table: "ChatMemberships");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "TaskChatMemberships",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChatMemberships",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskChatMemberships",
                table: "TaskChatMemberships",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatMemberships",
                table: "ChatMemberships",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserChatSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateLastViewing = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserChatSessions_ChatMemberships_ChatMembershipId",
                        column: x => x.ChatMembershipId,
                        principalTable: "ChatMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChatSessions_UserSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTaskChatSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatMembershipId = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "IX_TaskChatMemberships_ParticipantId",
                table: "TaskChatMemberships",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMemberships_UserId",
                table: "ChatMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChatSessions_ChatMembershipId",
                table: "UserChatSessions",
                column: "ChatMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChatSessions_SessionId",
                table: "UserChatSessions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTaskChatSessions_ChatMembershipId",
                table: "UserTaskChatSessions",
                column: "ChatMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTaskChatSessions_SessionId",
                table: "UserTaskChatSessions",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserChatSessions");

            migrationBuilder.DropTable(
                name: "UserTaskChatSessions");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskChatMemberships",
                table: "TaskChatMemberships");

            migrationBuilder.DropIndex(
                name: "IX_TaskChatMemberships_ParticipantId",
                table: "TaskChatMemberships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatMemberships",
                table: "ChatMemberships");

            migrationBuilder.DropIndex(
                name: "IX_ChatMemberships_UserId",
                table: "ChatMemberships");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TaskChatMemberships");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChatMemberships");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskChatMemberships",
                table: "TaskChatMemberships",
                columns: new[] { "ParticipantId", "ChatId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatMemberships",
                table: "ChatMemberships",
                columns: new[] { "UserId", "ChatId" });
        }
    }
}
