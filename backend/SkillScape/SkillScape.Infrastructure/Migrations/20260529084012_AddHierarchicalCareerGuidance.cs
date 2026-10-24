using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHierarchicalCareerGuidance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CareerPaths",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    ParentCareerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StreamType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SkillsRequired = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FutureScope = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalaryDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndustryGrowth = table.Column<double>(type: "float", nullable: false),
                    RoadmapJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Certifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Colleges = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DemandIndex = table.Column<double>(type: "float", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerPaths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerPaths_CareerPaths_ParentCareerId",
                        column: x => x.ParentCareerId,
                        principalTable: "CareerPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HierarchicalQuizQuestions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetCareerTags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HierarchyLevel = table.Column<int>(type: "int", nullable: false),
                    StreamType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AptitudeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HierarchicalQuizQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCareerProfiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SelectedHierarchy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuizAnswersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecommendedCareerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    PredictedSalary = table.Column<double>(type: "float", nullable: false),
                    RoadmapJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SkillGapJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookmarkedPaths = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCareerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCareerProfiles_CareerPaths_RecommendedCareerId",
                        column: x => x.RecommendedCareerId,
                        principalTable: "CareerPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCareerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareerPaths_ParentCareerId",
                table: "CareerPaths",
                column: "ParentCareerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCareerProfiles_RecommendedCareerId",
                table: "UserCareerProfiles",
                column: "RecommendedCareerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCareerProfiles_UserId",
                table: "UserCareerProfiles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HierarchicalQuizQuestions");

            migrationBuilder.DropTable(
                name: "UserCareerProfiles");

            migrationBuilder.DropTable(
                name: "CareerPaths");
        }
    }
}
