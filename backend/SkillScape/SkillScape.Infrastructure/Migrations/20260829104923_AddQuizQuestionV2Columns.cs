using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizQuestionV2Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubdomainEvidenceJson",
                table: "QuizResultsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TopSubdomainsJson",
                table: "QuizResultsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompetencyId",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConceptId",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubdomainIdsJson",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubdomainEvidenceJson",
                table: "QuizResultsV2");

            migrationBuilder.DropColumn(
                name: "TopSubdomainsJson",
                table: "QuizResultsV2");

            migrationBuilder.DropColumn(
                name: "CompetencyId",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "ConceptId",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "SubdomainIdsJson",
                table: "QuizQuestionsV2");
        }
    }
}
