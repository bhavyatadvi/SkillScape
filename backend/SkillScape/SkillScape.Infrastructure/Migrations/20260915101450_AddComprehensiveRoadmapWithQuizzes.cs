using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComprehensiveRoadmapWithQuizzes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                table: "RoadmapTopics",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "RoadmapTopics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RoadmapBlueprints",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CareerKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CareerTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    GeneratedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceModel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RawPromptHash = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapBlueprints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoadmapModuleQuizAttempts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModuleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    Passed = table.Column<bool>(type: "bit", nullable: false),
                    AnsweredJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoadmapModuleQuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoadmapModuleQuizAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapPhases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BlueprintId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PhaseNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedDuration = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapPhases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapPhases_RoadmapBlueprints_BlueprintId",
                        column: x => x.BlueprintId,
                        principalTable: "RoadmapBlueprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSavedRoadmaps",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CareerKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CareerTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlueprintId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastOpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSavedRoadmaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSavedRoadmaps_RoadmapBlueprints_BlueprintId",
                        column: x => x.BlueprintId,
                        principalTable: "RoadmapBlueprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSavedRoadmaps_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapModules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PhaseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    EstimatedHours = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapModules_RoadmapPhases_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "RoadmapPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapModuleQuizzes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModuleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PassScorePercentage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapModuleQuizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapModuleQuizzes_RoadmapModules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "RoadmapModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapQuizQuestions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QuizId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectOptionIndex = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DifficultyTag = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapQuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapQuizQuestions_RoadmapModuleQuizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "RoadmapModuleQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapBlueprints_CareerKey",
                table: "RoadmapBlueprints",
                column: "CareerKey");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapModuleQuizzes_ModuleId",
                table: "RoadmapModuleQuizzes",
                column: "ModuleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapModules_PhaseId",
                table: "RoadmapModules",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapPhases_BlueprintId",
                table: "RoadmapPhases",
                column: "BlueprintId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapQuizQuestions_QuizId",
                table: "RoadmapQuizQuestions",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoadmapModuleQuizAttempts_UserId_ModuleId",
                table: "UserRoadmapModuleQuizAttempts",
                columns: new[] { "UserId", "ModuleId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSavedRoadmaps_BlueprintId",
                table: "UserSavedRoadmaps",
                column: "BlueprintId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSavedRoadmaps_UserId_CareerKey",
                table: "UserSavedRoadmaps",
                columns: new[] { "UserId", "CareerKey" },
                unique: true);

            migrationBuilder.AddColumn<string>(
                name: "RoadmapModuleId",
                table: "RoadmapTopics",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModuleId",
                table: "RoadmapTopics",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapTopics_RoadmapModuleId",
                table: "RoadmapTopics",
                column: "RoadmapModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoadmapTopics_RoadmapModules_RoadmapModuleId",
                table: "RoadmapTopics",
                column: "RoadmapModuleId",
                principalTable: "RoadmapModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoadmapTopics_RoadmapModules_RoadmapModuleId",
                table: "RoadmapTopics");

            migrationBuilder.DropIndex(
                name: "IX_RoadmapTopics_RoadmapModuleId",
                table: "RoadmapTopics");

            migrationBuilder.DropColumn(
                name: "RoadmapModuleId",
                table: "RoadmapTopics");

            migrationBuilder.DropTable(
                name: "RoadmapQuizQuestions");

            migrationBuilder.DropTable(
                name: "UserRoadmapModuleQuizAttempts");

            migrationBuilder.DropTable(
                name: "UserSavedRoadmaps");

            migrationBuilder.DropTable(
                name: "RoadmapModuleQuizzes");

            migrationBuilder.DropTable(
                name: "RoadmapModules");

            migrationBuilder.DropTable(
                name: "RoadmapPhases");

            migrationBuilder.DropTable(
                name: "RoadmapBlueprints");

            migrationBuilder.DropColumn(
                name: "IsOptional",
                table: "RoadmapTopics");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "RoadmapTopics");
        }
    }
}
