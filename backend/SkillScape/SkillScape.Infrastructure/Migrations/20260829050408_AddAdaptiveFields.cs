using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdaptiveFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dimension",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DomainId",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FieldId",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Intent",
                table: "QuizQuestionsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StateJson",
                table: "AdaptiveQuizSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dimension",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "DomainId",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "FieldId",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "Intent",
                table: "QuizQuestionsV2");

            migrationBuilder.DropColumn(
                name: "StateJson",
                table: "AdaptiveQuizSessions");
        }
    }
}
