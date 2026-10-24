using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainToPeerRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "UserRoadmapDismissals",
            //     columns: table => new
            //     {
            //         Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //         UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //         CareerKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //         DismissedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_UserRoadmapDismissals", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_UserRoadmapDismissals_Users_UserId",
            //             column: x => x.UserId,
            //             principalTable: "Users",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_UserRoadmapDismissals_UserId_CareerKey",
            //     table: "UserRoadmapDismissals",
            //     columns: new[] { "UserId", "CareerKey" },
            //     unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRoadmapDismissals");
        }
    }
}
