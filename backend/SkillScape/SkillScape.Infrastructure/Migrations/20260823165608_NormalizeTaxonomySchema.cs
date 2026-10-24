using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeTaxonomySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NormAbilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MeasurementType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormAbilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormCareerDomains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormCareerDomains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormCareerFamilies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormCareerFamilies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuingAuthority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormCertifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormCourses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DegreeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationYears = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormCourses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormDataSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormDataSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormEducationStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormEducationStages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormEntranceExams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizingBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForDegreeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormEntranceExams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormGovernmentExams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizingBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormGovernmentExams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormInterests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HollandCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormInterests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormModelVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Algorithm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidationAccuracy = table.Column<double>(type: "float", nullable: false),
                    BlobStoragePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormModelVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormQuestionSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenceUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormQuestionSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormQuizSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormQuizSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormSkills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormSubjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingDataRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DatasetVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EducationStage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stream = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MathematicsScore = table.Column<float>(type: "real", nullable: false),
                    PhysicsScore = table.Column<float>(type: "real", nullable: false),
                    ProgrammingInterest = table.Column<float>(type: "real", nullable: false),
                    SpatialReasoning = table.Column<float>(type: "real", nullable: false),
                    HandsOnInterest = table.Column<float>(type: "real", nullable: false),
                    QuestionResponsesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SkillSignalsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DomainLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubFieldLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecializationLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CareerFamilyLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupationLabel = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingDataRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NormCareerFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DomainId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormCareerFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormCareerFields_NormCareerDomains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "NormCareerDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormOccupations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamilyId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OnetCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EscoCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormOccupations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormOccupations_NormCareerFamilies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "NormCareerFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormDataIngestionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataSourceId = table.Column<int>(type: "int", nullable: false),
                    RunStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordsProcessed = table.Column<int>(type: "int", nullable: false),
                    ErrorLogs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormDataIngestionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormDataIngestionLogs_NormDataSources_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "NormDataSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormQuestionBanks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormQuestionBanks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormQuestionBanks_NormQuestionSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "NormQuestionSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NormPredictionResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    ModelVersionId = table.Column<int>(type: "int", nullable: false),
                    PredictionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormPredictionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormPredictionResults_NormModelVersions_ModelVersionId",
                        column: x => x.ModelVersionId,
                        principalTable: "NormModelVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NormPredictionResults_NormQuizSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "NormQuizSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormCareerSubFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormCareerSubFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormCareerSubFields_NormCareerFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "NormCareerFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormCareerEducationMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OccupationId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormCareerEducationMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormCareerEducationMappings_NormCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "NormCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NormCareerEducationMappings_NormOccupations_OccupationId",
                        column: x => x.OccupationId,
                        principalTable: "NormOccupations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormCareerSkillMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OccupationId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    ProficiencyLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormCareerSkillMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormCareerSkillMappings_NormOccupations_OccupationId",
                        column: x => x.OccupationId,
                        principalTable: "NormOccupations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NormCareerSkillMappings_NormSkills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "NormSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormQuestionFieldMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    DiscriminatorWeight = table.Column<double>(type: "float", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormQuestionFieldMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormQuestionFieldMappings_NormCareerFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "NormCareerFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NormQuestionFieldMappings_NormQuestionBanks_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "NormQuestionBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormQuestionOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrectAnswer = table.Column<bool>(type: "bit", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormQuestionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormQuestionOptions_NormQuestionBanks_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "NormQuestionBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormCareerSpecializations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubFieldId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormCareerSpecializations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormCareerSpecializations_NormCareerSubFields_SubFieldId",
                        column: x => x.SubFieldId,
                        principalTable: "NormCareerSubFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormQuestionSignals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OptionId = table.Column<int>(type: "int", nullable: false),
                    DimensionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DimensionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormQuestionSignals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormQuestionSignals_NormQuestionOptions_OptionId",
                        column: x => x.OptionId,
                        principalTable: "NormQuestionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormQuizResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionId = table.Column<int>(type: "int", nullable: false),
                    TimeTakenSeconds = table.Column<double>(type: "float", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormQuizResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormQuizResponses_NormQuestionBanks_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "NormQuestionBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NormQuizResponses_NormQuestionOptions_OptionId",
                        column: x => x.OptionId,
                        principalTable: "NormQuestionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NormQuizResponses_NormQuizSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "NormQuizSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NormCareerOccupationMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecializationId = table.Column<int>(type: "int", nullable: false),
                    OccupationId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NormCareerOccupationMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NormCareerOccupationMappings_NormCareerSpecializations_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "NormCareerSpecializations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NormCareerOccupationMappings_NormOccupations_OccupationId",
                        column: x => x.OccupationId,
                        principalTable: "NormOccupations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NormAbilities_EntityId_Version",
                table: "NormAbilities",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerDomains_EntityId_Version",
                table: "NormCareerDomains",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerEducationMappings_CourseId",
                table: "NormCareerEducationMappings",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerEducationMappings_EntityId_Version",
                table: "NormCareerEducationMappings",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerEducationMappings_OccupationId",
                table: "NormCareerEducationMappings",
                column: "OccupationId");

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerFamilies_EntityId_Version",
                table: "NormCareerFamilies",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerFields_DomainId",
                table: "NormCareerFields",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerFields_EntityId_Version",
                table: "NormCareerFields",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerOccupationMappings_EntityId_Version",
                table: "NormCareerOccupationMappings",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerOccupationMappings_OccupationId",
                table: "NormCareerOccupationMappings",
                column: "OccupationId");

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerOccupationMappings_SpecializationId",
                table: "NormCareerOccupationMappings",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerSkillMappings_EntityId_Version",
                table: "NormCareerSkillMappings",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerSkillMappings_OccupationId",
                table: "NormCareerSkillMappings",
                column: "OccupationId");

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerSkillMappings_SkillId",
                table: "NormCareerSkillMappings",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerSpecializations_EntityId_Version",
                table: "NormCareerSpecializations",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerSpecializations_SubFieldId",
                table: "NormCareerSpecializations",
                column: "SubFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerSubFields_EntityId_Version",
                table: "NormCareerSubFields",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormCareerSubFields_FieldId",
                table: "NormCareerSubFields",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_NormCertifications_EntityId_Version",
                table: "NormCertifications",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormCourses_EntityId_Version",
                table: "NormCourses",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormDataIngestionLogs_DataSourceId",
                table: "NormDataIngestionLogs",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_NormDataIngestionLogs_EntityId_Version",
                table: "NormDataIngestionLogs",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormDataSources_EntityId_Version",
                table: "NormDataSources",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormEducationStages_EntityId_Version",
                table: "NormEducationStages",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormEntranceExams_EntityId_Version",
                table: "NormEntranceExams",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormGovernmentExams_EntityId_Version",
                table: "NormGovernmentExams",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormInterests_EntityId_Version",
                table: "NormInterests",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormModelVersions_EntityId_Version",
                table: "NormModelVersions",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormOccupations_EntityId_Version",
                table: "NormOccupations",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormOccupations_FamilyId",
                table: "NormOccupations",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_NormPredictionResults_EntityId_Version",
                table: "NormPredictionResults",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormPredictionResults_ModelVersionId",
                table: "NormPredictionResults",
                column: "ModelVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_NormPredictionResults_SessionId",
                table: "NormPredictionResults",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_NormQuestionBanks_EntityId_Version",
                table: "NormQuestionBanks",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormQuestionBanks_SourceId",
                table: "NormQuestionBanks",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_NormQuestionFieldMappings_EntityId_Version",
                table: "NormQuestionFieldMappings",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormQuestionFieldMappings_FieldId",
                table: "NormQuestionFieldMappings",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_NormQuestionFieldMappings_QuestionId",
                table: "NormQuestionFieldMappings",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_NormQuestionOptions_EntityId_Version",
                table: "NormQuestionOptions",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormQuestionOptions_QuestionId",
                table: "NormQuestionOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_NormQuestionSignals_EntityId_Version",
                table: "NormQuestionSignals",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormQuestionSignals_OptionId",
                table: "NormQuestionSignals",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_NormQuestionSources_EntityId_Version",
                table: "NormQuestionSources",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormQuizResponses_EntityId_Version",
                table: "NormQuizResponses",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormQuizResponses_OptionId",
                table: "NormQuizResponses",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_NormQuizResponses_QuestionId",
                table: "NormQuizResponses",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_NormQuizResponses_SessionId",
                table: "NormQuizResponses",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_NormQuizSessions_EntityId_Version",
                table: "NormQuizSessions",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormSkills_EntityId_Version",
                table: "NormSkills",
                columns: new[] { "EntityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NormSubjects_EntityId_Version",
                table: "NormSubjects",
                columns: new[] { "EntityId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NormAbilities");

            migrationBuilder.DropTable(
                name: "NormCareerEducationMappings");

            migrationBuilder.DropTable(
                name: "NormCareerOccupationMappings");

            migrationBuilder.DropTable(
                name: "NormCareerSkillMappings");

            migrationBuilder.DropTable(
                name: "NormCertifications");

            migrationBuilder.DropTable(
                name: "NormDataIngestionLogs");

            migrationBuilder.DropTable(
                name: "NormEducationStages");

            migrationBuilder.DropTable(
                name: "NormEntranceExams");

            migrationBuilder.DropTable(
                name: "NormGovernmentExams");

            migrationBuilder.DropTable(
                name: "NormInterests");

            migrationBuilder.DropTable(
                name: "NormPredictionResults");

            migrationBuilder.DropTable(
                name: "NormQuestionFieldMappings");

            migrationBuilder.DropTable(
                name: "NormQuestionSignals");

            migrationBuilder.DropTable(
                name: "NormQuizResponses");

            migrationBuilder.DropTable(
                name: "NormSubjects");

            migrationBuilder.DropTable(
                name: "TrainingDataRecords");

            migrationBuilder.DropTable(
                name: "NormCourses");

            migrationBuilder.DropTable(
                name: "NormCareerSpecializations");

            migrationBuilder.DropTable(
                name: "NormOccupations");

            migrationBuilder.DropTable(
                name: "NormSkills");

            migrationBuilder.DropTable(
                name: "NormDataSources");

            migrationBuilder.DropTable(
                name: "NormModelVersions");

            migrationBuilder.DropTable(
                name: "NormQuestionOptions");

            migrationBuilder.DropTable(
                name: "NormQuizSessions");

            migrationBuilder.DropTable(
                name: "NormCareerSubFields");

            migrationBuilder.DropTable(
                name: "NormCareerFamilies");

            migrationBuilder.DropTable(
                name: "NormQuestionBanks");

            migrationBuilder.DropTable(
                name: "NormCareerFields");

            migrationBuilder.DropTable(
                name: "NormQuestionSources");

            migrationBuilder.DropTable(
                name: "NormCareerDomains");
        }
    }
}
