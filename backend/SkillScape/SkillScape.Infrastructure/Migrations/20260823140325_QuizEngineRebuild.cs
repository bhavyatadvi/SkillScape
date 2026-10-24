using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuizEngineRebuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProgressions");


            migrationBuilder.CreateTable(
                name: "CareerTaxonomyNodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ParentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerTaxonomyNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EligibilityRules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TargetDomain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiredSubject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiredLevel = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizPhases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizPhases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestionSignals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QuizOptionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TargetDomain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestionSignals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizQuestionSignals_QuizOptions_QuizOptionId",
                        column: x => x.QuizOptionId,
                        principalTable: "QuizOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizPhases_Key",
                table: "QuizPhases",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestionSignals_QuizOptionId",
                table: "QuizQuestionSignals",
                column: "QuizOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CareerTaxonomyNodes");

            migrationBuilder.DropTable(
                name: "EligibilityRules");

            migrationBuilder.DropTable(
                name: "QuizPhases");

            migrationBuilder.DropTable(
                name: "QuizQuestionSignals");


            migrationBuilder.CreateTable(
                name: "UserProgressions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CareerDomainId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProgressPercentage = table.Column<double>(type: "float", nullable: false),
                    SkillsCompleted = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalSkills = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    XPInDomain = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgressions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProgressions_CareerDomains_CareerDomainId",
                        column: x => x.CareerDomainId,
                        principalTable: "CareerDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProgressions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProgressions_CareerDomainId",
                table: "UserProgressions",
                column: "CareerDomainId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgressions_UserId_CareerDomainId",
                table: "UserProgressions",
                columns: new[] { "UserId", "CareerDomainId" },
                unique: true);
        }
    }
}
