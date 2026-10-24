using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdaptiveArchitecturePhase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompetencyDimensionsJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InterestDimensionsJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KnowledgeAreasJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuestionTagsJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScoringWeightsJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubdomainsJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkTaskDimensionsJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CareerScoresJson",
                table: "AdaptiveQuizSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompetencyScoresJson",
                table: "AdaptiveQuizSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "ConfidenceScore",
                table: "AdaptiveQuizSessions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Mode",
                table: "AdaptiveQuizSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuestionPlanJson",
                table: "AdaptiveQuizSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubdomainScoresJson",
                table: "AdaptiveQuizSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetCareersJson",
                table: "AdaptiveQuizSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "QuizQuestionRecordsV2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConceptId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompetencyTested = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscriminatorTargetsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AskedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestionRecordsV2", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizQuestionRecordsV2");

            migrationBuilder.DropColumn(
                name: "CompetencyDimensionsJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "InterestDimensionsJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "KnowledgeAreasJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "QuestionTagsJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "ScoringWeightsJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "SubdomainsJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "WorkTaskDimensionsJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "CareerScoresJson",
                table: "AdaptiveQuizSessions");

            migrationBuilder.DropColumn(
                name: "CompetencyScoresJson",
                table: "AdaptiveQuizSessions");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "AdaptiveQuizSessions");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "AdaptiveQuizSessions");

            migrationBuilder.DropColumn(
                name: "QuestionPlanJson",
                table: "AdaptiveQuizSessions");

            migrationBuilder.DropColumn(
                name: "SubdomainScoresJson",
                table: "AdaptiveQuizSessions");

            migrationBuilder.DropColumn(
                name: "TargetCareersJson",
                table: "AdaptiveQuizSessions");
        }
    }
}
