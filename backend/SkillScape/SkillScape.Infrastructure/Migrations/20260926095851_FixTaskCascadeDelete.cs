using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTaskCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningCircleNotes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CircleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningCircleNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningCircleNotes_LearningCircles_CircleId",
                        column: x => x.CircleId,
                        principalTable: "LearningCircles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningCircleNotes_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningCircleTasks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CircleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningCircleTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningCircleTasks_LearningCircles_CircleId",
                        column: x => x.CircleId,
                        principalTable: "LearningCircles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningCircleTasks_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearningCircleTasks_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningCircleNotes_CircleId",
                table: "LearningCircleNotes",
                column: "CircleId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningCircleNotes_UpdatedByUserId",
                table: "LearningCircleNotes",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningCircleTasks_AssignedToUserId",
                table: "LearningCircleTasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningCircleTasks_CircleId",
                table: "LearningCircleTasks",
                column: "CircleId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningCircleTasks_CreatedByUserId",
                table: "LearningCircleTasks",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningCircleNotes");

            migrationBuilder.DropTable(
                name: "LearningCircleTasks");
        }
    }
}
