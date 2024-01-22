using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace old_planner_api.Migrations
{
    /// <inheritdoc />
    public partial class init_old_version_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    PriorityOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    HexColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeletedTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExistBeforeDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeletedTasks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Drafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    HexColor = table.Column<string>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedTaskId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drafts_Tasks_ModifiedTaskId",
                        column: x => x.ModifiedTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeletedTasks_TaskId",
                table: "DeletedTasks",
                column: "TaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drafts_ModifiedTaskId",
                table: "Drafts",
                column: "ModifiedTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeletedTasks");

            migrationBuilder.DropTable(
                name: "Drafts");

            migrationBuilder.DropTable(
                name: "Tasks");
        }
    }
}
