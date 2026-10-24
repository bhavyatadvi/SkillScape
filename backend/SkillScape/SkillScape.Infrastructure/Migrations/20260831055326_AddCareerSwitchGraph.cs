using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerSwitchGraph : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NormDataIngestionLogs_NormDataSources_DataSourceId",
                table: "NormDataIngestionLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NormDataSources",
                table: "NormDataSources");

            migrationBuilder.DropIndex(
                name: "IX_NormDataSources_EntityId_Version",
                table: "NormDataSources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NormDataIngestionLogs",
                table: "NormDataIngestionLogs");

            migrationBuilder.DropIndex(
                name: "IX_NormDataIngestionLogs_EntityId_Version",
                table: "NormDataIngestionLogs");

            migrationBuilder.RenameTable(
                name: "NormDataSources",
                newName: "NormDataSource");

            migrationBuilder.RenameTable(
                name: "NormDataIngestionLogs",
                newName: "NormDataIngestionLog");

            migrationBuilder.RenameIndex(
                name: "IX_NormDataIngestionLogs_DataSourceId",
                table: "NormDataIngestionLog",
                newName: "IX_NormDataIngestionLog_DataSourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NormDataSource",
                table: "NormDataSource",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NormDataIngestionLog",
                table: "NormDataIngestionLog",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CareerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NodeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinimumEducation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRegulated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareerSwitchQuestions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DomainDiscriminator = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerSwitchQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransferableSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferableSkills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareerTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromCareerId = table.Column<int>(type: "int", nullable: false),
                    ToCareerId = table.Column<int>(type: "int", nullable: false),
                    TransitionDifficulty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransitionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompatibilityScore = table.Column<double>(type: "float", nullable: false),
                    WhyItFits = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PotentialChallenges = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExistingSkillsUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewSkillsRequired = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntryRoles = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerTransitions_CareerProfiles_FromCareerId",
                        column: x => x.FromCareerId,
                        principalTable: "CareerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CareerTransitions_CareerProfiles_ToCareerId",
                        column: x => x.ToCareerId,
                        principalTable: "CareerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CareerSwitchQuestionOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CareerSwitchQuestionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AffectedCareerNodeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerSwitchQuestionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerSwitchQuestionOptions_CareerSwitchQuestions_CareerSwitchQuestionId",
                        column: x => x.CareerSwitchQuestionId,
                        principalTable: "CareerSwitchQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareerSwitchQuestionOptions_CareerSwitchQuestionId",
                table: "CareerSwitchQuestionOptions",
                column: "CareerSwitchQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerTransitions_FromCareerId",
                table: "CareerTransitions",
                column: "FromCareerId");

            migrationBuilder.CreateIndex(
                name: "IX_CareerTransitions_ToCareerId",
                table: "CareerTransitions",
                column: "ToCareerId");

            migrationBuilder.AddForeignKey(
                name: "FK_NormDataIngestionLog_NormDataSource_DataSourceId",
                table: "NormDataIngestionLog",
                column: "DataSourceId",
                principalTable: "NormDataSource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NormDataIngestionLog_NormDataSource_DataSourceId",
                table: "NormDataIngestionLog");

            migrationBuilder.DropTable(
                name: "CareerSwitchQuestionOptions");

            migrationBuilder.DropTable(
                name: "CareerTransitions");

            migrationBuilder.DropTable(
                name: "TransferableSkills");

            migrationBuilder.DropTable(
                name: "CareerSwitchQuestions");

            migrationBuilder.DropTable(
                name: "CareerProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NormDataSource",
                table: "NormDataSource");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NormDataIngestionLog",
                table: "NormDataIngestionLog");

            migrationBuilder.RenameTable(
                name: "NormDataSource",
                newName: "NormDataSources");

            migrationBuilder.RenameTable(
                name: "NormDataIngestionLog",
                newName: "NormDataIngestionLogs");

            migrationBuilder.RenameIndex(
                name: "IX_NormDataIngestionLog_DataSourceId",
                table: "NormDataIngestionLogs",
                newName: "IX_NormDataIngestionLogs_DataSourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NormDataSources",
                table: "NormDataSources",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NormDataIngestionLogs",
                table: "NormDataIngestionLogs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_NormDataSources_EntityId_Version",
                table: "NormDataSources",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormDataIngestionLogs_EntityId_Version",
                table: "NormDataIngestionLogs",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NormDataIngestionLogs_NormDataSources_DataSourceId",
                table: "NormDataIngestionLogs",
                column: "DataSourceId",
                principalTable: "NormDataSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
