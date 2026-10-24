using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerDiscoveryKnowledgeGraph : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CareerGrowthGraphJson",
                table: "UserCareerProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StrengthsWeaknessesJson",
                table: "UserCareerProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TopMatchesJson",
                table: "UserCareerProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhyNotRecommended",
                table: "UserCareerProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhyRecommended",
                table: "UserCareerProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "AutomationRisk",
                table: "CareerPaths",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CareerFlexibility",
                table: "CareerPaths",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "DifficultyLevel",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "EntrepreneurshipPotential",
                table: "CareerPaths",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "LearningResources",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PersonalityMatch",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecommendedSubjects",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelatedCareerIds",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "RemoteWorkPossibility",
                table: "CareerPaths",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "RequiredDegrees",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoadmapTimeline",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TopCompanies",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhatYouWillStudy",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkEnvironment",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "YoutubeResources",
                table: "CareerPaths",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CareerGrowthGraphJson",
                table: "UserCareerProfiles");

            migrationBuilder.DropColumn(
                name: "StrengthsWeaknessesJson",
                table: "UserCareerProfiles");

            migrationBuilder.DropColumn(
                name: "TopMatchesJson",
                table: "UserCareerProfiles");

            migrationBuilder.DropColumn(
                name: "WhyNotRecommended",
                table: "UserCareerProfiles");

            migrationBuilder.DropColumn(
                name: "WhyRecommended",
                table: "UserCareerProfiles");

            migrationBuilder.DropColumn(
                name: "AutomationRisk",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "CareerFlexibility",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "DifficultyLevel",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "EntrepreneurshipPotential",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "LearningResources",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "PersonalityMatch",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "RecommendedSubjects",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "RelatedCareerIds",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "RemoteWorkPossibility",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "RequiredDegrees",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "RoadmapTimeline",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "TopCompanies",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "WhatYouWillStudy",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "WorkEnvironment",
                table: "CareerPaths");

            migrationBuilder.DropColumn(
                name: "YoutubeResources",
                table: "CareerPaths");
        }
    }
}
