using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCentralCareerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- 1. Drop old foreign keys if present
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CareerTransitions_CareerProfiles_FromCareerId')
                    ALTER TABLE [CareerTransitions] DROP CONSTRAINT [FK_CareerTransitions_CareerProfiles_FromCareerId];

                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CareerTransitions_CareerProfiles_ToCareerId')
                    ALTER TABLE [CareerTransitions] DROP CONSTRAINT [FK_CareerTransitions_CareerProfiles_ToCareerId];

                -- 2. If old CareerProfiles table exists and CareerSwitchNodes table does NOT exist, RENAME CareerProfiles -> CareerSwitchNodes
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CareerProfiles') 
                   AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CareerSwitchNodes')
                BEGIN
                    EXEC sp_rename 'CareerProfiles', 'CareerSwitchNodes';
                END

                -- 3. If primary key constraint PK_CareerProfiles exists, rename it to PK_CareerSwitchNodes
                IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_CareerProfiles')
                BEGIN
                    EXEC sp_rename 'PK_CareerProfiles', 'PK_CareerSwitchNodes';
                END

                -- 4. If CareerSwitchNodes table does not exist yet (e.g. fresh environment), create it
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CareerSwitchNodes')
                BEGIN
                    CREATE TABLE [CareerSwitchNodes] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [NodeId] nvarchar(max) NOT NULL,
                        [Title] nvarchar(max) NOT NULL,
                        [Domain] nvarchar(max) NOT NULL,
                        [Industry] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [MinimumEducation] nvarchar(max) NOT NULL,
                        [IsRegulated] bit NOT NULL,
                        CONSTRAINT [PK_CareerSwitchNodes] PRIMARY KEY ([Id])
                    );
                END

                -- 5. Create CentralCareerProfiles table if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CentralCareerProfiles')
                BEGIN
                    CREATE TABLE [CentralCareerProfiles] (
                        [Id] nvarchar(450) NOT NULL,
                        [UserId] nvarchar(450) NOT NULL,
                        [EducationLevel] nvarchar(max) NOT NULL,
                        [Degree] nvarchar(max) NOT NULL,
                        [BroadField] nvarchar(max) NOT NULL,
                        [Specialization] nvarchar(max) NOT NULL,
                        [Institution] nvarchar(max) NOT NULL,
                        [GraduationYear] int NULL,
                        [AdditionalQualifications] nvarchar(max) NOT NULL,
                        [EducationStatus] nvarchar(max) NOT NULL,
                        [CurrentEmploymentStatus] nvarchar(max) NOT NULL,
                        [CurrentOccupation] nvarchar(max) NOT NULL,
                        [CurrentRole] nvarchar(max) NOT NULL,
                        [CurrentIndustry] nvarchar(max) NOT NULL,
                        [OrganizationType] nvarchar(max) NOT NULL,
                        [ExperienceYears] float NULL,
                        [RelevantExperience] nvarchar(max) NOT NULL,
                        [PreviousRoles] nvarchar(max) NOT NULL,
                        [PreviousIndustries] nvarchar(max) NOT NULL,
                        [CurrentSkillsJson] nvarchar(max) NOT NULL,
                        [CertificationsJson] nvarchar(max) NOT NULL,
                        [CareerLikesJson] nvarchar(max) NOT NULL,
                        [CareerDislikesJson] nvarchar(max) NOT NULL,
                        [WorkPreferencesJson] nvarchar(max) NOT NULL,
                        [MotivationsJson] nvarchar(max) NOT NULL,
                        [ConstraintsJson] nvarchar(max) NOT NULL,
                        [CareerChangePreference] nvarchar(max) NOT NULL,
                        [Location] nvarchar(max) NOT NULL,
                        [PreferredWorkMode] nvarchar(max) NOT NULL,
                        [SalaryRange] nvarchar(max) NOT NULL,
                        [CareerGoals] nvarchar(max) NOT NULL,
                        [ProfileSource] nvarchar(max) NOT NULL,
                        [ResumeFileName] nvarchar(max) NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_CentralCareerProfiles] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_CentralCareerProfiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
                    );

                    CREATE UNIQUE INDEX [IX_CentralCareerProfiles_UserId] ON [CentralCareerProfiles] ([UserId]);
                END

                -- 6. Add foreign keys on CareerTransitions referencing CareerSwitchNodes
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CareerTransitions_CareerSwitchNodes_FromCareerId')
                BEGIN
                    ALTER TABLE [CareerTransitions] ADD CONSTRAINT [FK_CareerTransitions_CareerSwitchNodes_FromCareerId] 
                        FOREIGN KEY ([FromCareerId]) REFERENCES [CareerSwitchNodes] ([Id]);
                END

                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CareerTransitions_CareerSwitchNodes_ToCareerId')
                BEGIN
                    ALTER TABLE [CareerTransitions] ADD CONSTRAINT [FK_CareerTransitions_CareerSwitchNodes_ToCareerId] 
                        FOREIGN KEY ([ToCareerId]) REFERENCES [CareerSwitchNodes] ([Id]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CareerTransitions_CareerSwitchNodes_FromCareerId')
                    ALTER TABLE [CareerTransitions] DROP CONSTRAINT [FK_CareerTransitions_CareerSwitchNodes_FromCareerId];

                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CareerTransitions_CareerSwitchNodes_ToCareerId')
                    ALTER TABLE [CareerTransitions] DROP CONSTRAINT [FK_CareerTransitions_CareerSwitchNodes_ToCareerId];

                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CentralCareerProfiles')
                    DROP TABLE [CentralCareerProfiles];

                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CareerSwitchNodes') 
                   AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CareerProfiles')
                BEGIN
                    EXEC sp_rename 'CareerSwitchNodes', 'CareerProfiles';
                END

                IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_CareerSwitchNodes')
                BEGIN
                    EXEC sp_rename 'PK_CareerSwitchNodes', 'PK_CareerProfiles';
                END
            ");
        }
    }
}
