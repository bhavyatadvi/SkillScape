using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaxonomyV2Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CareerWeightsJson",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DimensionWeightsJson",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EducationLevelsJson",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subdomain",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AptitudeDimensionsJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EligibleEducationLevelsJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subdomain",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TraitWeightsJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkStyleDimensionsJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AssessmentContextJson",
                table: "AdaptiveQuizSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CareerWeightsJson",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "DimensionWeightsJson",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "EducationLevelsJson",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "Subdomain",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "AptitudeDimensionsJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "EligibleEducationLevelsJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "Subdomain",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "TraitWeightsJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "WorkStyleDimensionsJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "AssessmentContextJson",
                table: "AdaptiveQuizSessions");
        }
    }
}
