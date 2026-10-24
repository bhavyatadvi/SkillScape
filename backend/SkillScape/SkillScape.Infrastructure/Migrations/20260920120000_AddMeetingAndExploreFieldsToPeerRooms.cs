using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SkillScape.Infrastructure.Data;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260920120000_AddMeetingAndExploreFieldsToPeerRooms")]
    /// <inheritdoc />
    public partial class AddMeetingAndExploreFieldsToPeerRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MeetingUrl",
                table: "PeerStudyRooms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "PeerStudyRooms",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "PeerStudyRooms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GithubUrl",
                table: "PeerStudyRooms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeetingUrl",
                table: "PeerStudyRooms");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "PeerStudyRooms");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "PeerStudyRooms");

            migrationBuilder.DropColumn(
                name: "GithubUrl",
                table: "PeerStudyRooms");
        }
    }
}
