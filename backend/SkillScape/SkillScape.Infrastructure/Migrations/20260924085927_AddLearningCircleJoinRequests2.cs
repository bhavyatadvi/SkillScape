using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningCircleJoinRequests2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "LearningCircles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsMeetingActive",
                table: "LearningCircles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LearningCircleJoinRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CircleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningCircleJoinRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningCircleJoinRequests_LearningCircles_CircleId",
                        column: x => x.CircleId,
                        principalTable: "LearningCircles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningCircleJoinRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningCircleJoinRequests_CircleId",
                table: "LearningCircleJoinRequests",
                column: "CircleId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningCircleJoinRequests_UserId",
                table: "LearningCircleJoinRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningCircleJoinRequests");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "LearningCircles");

            migrationBuilder.DropColumn(
                name: "IsMeetingActive",
                table: "LearningCircles");
        }
    }
}
