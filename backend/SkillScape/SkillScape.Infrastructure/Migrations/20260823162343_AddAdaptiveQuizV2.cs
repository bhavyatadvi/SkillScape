using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdaptiveQuizV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizResponses_QuizOptions_QuizOptionId1",
                table: "QuizResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizResponses_QuizQuestions_QuizQuestionId1",
                table: "QuizResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuizResponses_QuizOptionId1",
                table: "QuizResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuizResponses_QuizQuestionId1",
                table: "QuizResponses");

            migrationBuilder.DropColumn(
                name: "QuizOptionId1",
                table: "QuizResponses");

            migrationBuilder.DropColumn(
                name: "QuizQuestionId1",
                table: "QuizResponses");

            migrationBuilder.AddColumn<string>(
                name: "CanonicalSignalsJson",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhaseApplicabilityJson",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxonomyNodeId",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxonomyPath",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AdaptiveQuizSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhaseKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubPhaseKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackgroundProfileJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProbabilityDistributionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnsweredQuestionIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdaptiveQuizSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareerTaxonomyV2",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhasesApplicableJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KeySubjectsRequired = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypicalRolesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredDegreesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecommendedCoursesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstitutionTypesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntranceExamsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalaryRangeJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndustryGrowthRate = table.Column<double>(type: "float", nullable: false),
                    DemandIndex = table.Column<double>(type: "float", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerTaxonomyV2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EligibilityConstraintsV2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxonomyNodeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhaseKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredSubjectCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredMinPerformance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsHardConstraint = table.Column<bool>(type: "bit", nullable: false),
                    SoftPenaltyFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityConstraintsV2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizOptionSignalsV2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionIndex = table.Column<int>(type: "int", nullable: false),
                    TaxonomyNodeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SignalWeight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizOptionSignalsV2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestionsV2",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PhaseKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubPhaseKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LanguageComplexity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetDiscriminatesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestionsV2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizResultsV2",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhaseKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryTaxonomyNodeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TopPredictionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: false),
                    EvidenceLogJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SkillGapJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EducationPathJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecommendedCoursesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EligibilityWarningsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RuleBasedScoresJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WhyRecommended = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizResultsV2", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdaptiveQuizSessions");

            migrationBuilder.DropTable(
                name: "CareerTaxonomyV2");

            migrationBuilder.DropTable(
                name: "EligibilityConstraintsV2");

            migrationBuilder.DropTable(
                name: "QuizOptionSignalsV2");

            migrationBuilder.DropTable(
                name: "QuizQuestionsV2");

            migrationBuilder.DropTable(
                name: "QuizResultsV2");

            migrationBuilder.DropColumn(
                name: "CanonicalSignalsJson",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "PhaseApplicabilityJson",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "TaxonomyNodeId",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "TaxonomyPath",
                table: "CareerPaths");

            migrationBuilder.AddColumn<string>(
                name: "QuizOptionId1",
                table: "QuizResponses",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuizQuestionId1",
                table: "QuizResponses",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizResponses_QuizOptionId1",
                table: "QuizResponses",
                column: "QuizOptionId1");

            migrationBuilder.CreateIndex(
                name: "IX_QuizResponses_QuizQuestionId1",
                table: "QuizResponses",
                column: "QuizQuestionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizResponses_QuizOptions_QuizOptionId1",
                table: "QuizResponses",
                column: "QuizOptionId1",
                principalTable: "QuizOptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizResponses_QuizQuestions_QuizQuestionId1",
                table: "QuizResponses",
                column: "QuizQuestionId1",
                principalTable: "QuizQuestions",
                principalColumn: "Id");
        }
    }
}
