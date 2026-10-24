using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerTaxonomyFieldsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataSource",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EntryLevelSalaryRange",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExperiencedSalaryRange",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GovernmentExamsApplicableJson",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastVerifiedDate",
                table: "CareerTaxonomyV2",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredExperience",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeToCompleteQualification",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeToJobReady",
                table: "CareerTaxonomyV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataSource",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "EntryLevelSalaryRange",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "ExperiencedSalaryRange",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "GovernmentExamsApplicableJson",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "LastVerifiedDate",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "RequiredExperience",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "TimeToCompleteQualification",
                table: "CareerTaxonomyV2");

            migrationBuilder.DropColumn(
                name: "TimeToJobReady",
                table: "CareerTaxonomyV2");
        }
    }
}
