using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningCircleTaskCompletions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningCircleTaskCompletions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TaskId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningCircleTaskCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningCircleTaskCompletions_LearningCircleTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "LearningCircleTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningCircleTaskCompletions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningCircleTaskCompletions_TaskId",
                table: "LearningCircleTaskCompletions",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningCircleTaskCompletions_UserId",
                table: "LearningCircleTaskCompletions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningCircleTaskCompletions");
        }
    }
}
