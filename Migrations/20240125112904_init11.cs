using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace old_planner_api.Migrations
{
    /// <inheritdoc />
    public partial class init11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tasks_TaskId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_DeletedTasks_TaskId",
                table: "DeletedTasks");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Tasks",
                newName: "DraftOfTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_TaskId",
                table: "Tasks",
                newName: "IX_Tasks_DraftOfTaskId");

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_DeletedTasks_TaskId",
                table: "DeletedTasks",
                column: "TaskId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tasks_DraftOfTaskId",
                table: "Tasks",
                column: "DraftOfTaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tasks_DraftOfTaskId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_DeletedTasks_TaskId",
                table: "DeletedTasks");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "DraftOfTaskId",
                table: "Tasks",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_DraftOfTaskId",
                table: "Tasks",
                newName: "IX_Tasks_TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedTasks_TaskId",
                table: "DeletedTasks",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tasks_TaskId",
                table: "Tasks",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
