using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryIntelligence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "CareerSalaryMappings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CareerPathId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SalaryBenchmarkRoleKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MappingMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MappingConfidence = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerSalaryMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerSalaryMappings_CareerPaths_CareerPathId",
                        column: x => x.CareerPathId,
                        principalTable: "CareerPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalaryBenchmarks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CanonicalRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupationFamily = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExperienceBand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    P25Salary = table.Column<double>(type: "float", nullable: false),
                    P50Salary = table.Column<double>(type: "float", nullable: false),
                    P75Salary = table.Column<double>(type: "float", nullable: false),
                    DataSourceCount = table.Column<int>(type: "int", nullable: false),
                    SourceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataQuality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryBenchmarks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalaryBenchmarkSources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SalaryBenchmarkId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RetrievedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SampleSize = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Methodology = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryBenchmarkSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryBenchmarkSources_SalaryBenchmarks_SalaryBenchmarkId",
                        column: x => x.SalaryBenchmarkId,
                        principalTable: "SalaryBenchmarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareerSalaryMappings_CareerPathId",
                table: "CareerSalaryMappings",
                column: "CareerPathId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBenchmarks_RoleKey",
                table: "SalaryBenchmarks",
                column: "RoleKey");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBenchmarkSources_SalaryBenchmarkId",
                table: "SalaryBenchmarkSources",
                column: "SalaryBenchmarkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CareerSalaryMappings");

            migrationBuilder.DropTable(
                name: "SalaryBenchmarkSources");

            migrationBuilder.DropTable(
                name: "SalaryBenchmarks");
        }
    }
}
