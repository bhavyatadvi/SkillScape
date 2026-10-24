using Microsoft.EntityFrameworkCore.Migrations;

using Microsoft.EntityFrameworkCore.Infrastructure;
using SkillScape.Infrastructure.Data;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPeerMatchingUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<string>(
            //    name: "LearningDomain",
            //    table: "Users",
            //    type: "nvarchar(256)",
            //    maxLength: 256,
            //    nullable: true);

            // migrationBuilder.AddColumn<string>(
            //    name: "LearningGoals",
            //    table: "Users",
            //    type: "nvarchar(1000)",
            //    maxLength: 1000,
            //    nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "Availability",
            //     table: "Users",
            //     type: "nvarchar(50)",
            //     maxLength: 50,
            //     nullable: false,
            //     defaultValue: "Flexible");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LearningDomain",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LearningGoals",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Availability",
                table: "Users");
        }
    }
}
