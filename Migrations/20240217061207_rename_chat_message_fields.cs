using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace old_planner_api.Migrations
{
    /// <inheritdoc />
    public partial class rename_chat_message_fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAtDate",
                table: "TaskChatMessages",
                newName: "SentAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "TaskChatMessages",
                newName: "CreatedAtDate");
        }
    }
}
