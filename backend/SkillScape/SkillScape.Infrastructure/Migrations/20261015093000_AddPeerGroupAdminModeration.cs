using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    public partial class AddPeerGroupAdminModeration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisabledByAdmin",
                table: "LearningCircles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AdminDisabledReason",
                table: "LearningCircles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AdminDisabledReason", table: "LearningCircles");
            migrationBuilder.DropColumn(name: "IsDisabledByAdmin", table: "LearningCircles");
        }
    }
}
