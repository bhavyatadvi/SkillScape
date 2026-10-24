using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKnowledgeGraph : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KgNodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtractionTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Confidence = table.Column<double>(type: "float", nullable: false),
                    LicenseMetadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KgNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KgEdges",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SourceNodeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TargetNodeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PropertiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtractionTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Confidence = table.Column<double>(type: "float", nullable: false),
                    LicenseMetadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KgEdges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KgEdges_KgNodes_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "KgNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KgEdges_KgNodes_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "KgNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KgEdges_SourceNodeId",
                table: "KgEdges",
                column: "SourceNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_KgEdges_TargetNodeId",
                table: "KgEdges",
                column: "TargetNodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KgEdges");

            migrationBuilder.DropTable(
                name: "KgNodes");
        }
    }
}
