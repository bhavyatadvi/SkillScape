using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRecommendedCareerFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCareerProfiles_CareerPaths_RecommendedCareerId",
                table: "UserCareerProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "RecommendedCareerId",
                table: "UserCareerProfiles",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCareerProfiles_CareerPaths_RecommendedCareerId",
                table: "UserCareerProfiles",
                column: "RecommendedCareerId",
                principalTable: "CareerPaths",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCareerProfiles_CareerPaths_RecommendedCareerId",
                table: "UserCareerProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "RecommendedCareerId",
                table: "UserCareerProfiles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCareerProfiles_CareerPaths_RecommendedCareerId",
                table: "UserCareerProfiles",
                column: "RecommendedCareerId",
                principalTable: "CareerPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
