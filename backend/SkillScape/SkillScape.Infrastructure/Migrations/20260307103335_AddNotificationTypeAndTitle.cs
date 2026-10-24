using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTypeAndTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_MentorshipProgressEntries_StudentId_RoadmapStage'
      AND object_id = OBJECT_ID('MentorshipProgressEntries')
)
BEGIN
    DROP INDEX [IX_MentorshipProgressEntries_StudentId_RoadmapStage] ON [MentorshipProgressEntries]
END");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Notifications', 'Title') IS NULL
BEGIN
    ALTER TABLE [Notifications] ADD [Title] nvarchar(max) NOT NULL CONSTRAINT [DF_Notifications_Title] DEFAULT N''
END");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Notifications', 'Type') IS NULL
BEGIN
    ALTER TABLE [Notifications] ADD [Type] nvarchar(max) NOT NULL CONSTRAINT [DF_Notifications_Type] DEFAULT N''
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_MentorshipProgressEntries_StudentId_MentorId'
      AND object_id = OBJECT_ID('MentorshipProgressEntries')
)
BEGIN
    CREATE UNIQUE INDEX [IX_MentorshipProgressEntries_StudentId_MentorId]
    ON [MentorshipProgressEntries]([StudentId], [MentorId])
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_MentorshipProgressEntries_StudentId_MentorId'
      AND object_id = OBJECT_ID('MentorshipProgressEntries')
)
BEGIN
    DROP INDEX [IX_MentorshipProgressEntries_StudentId_MentorId] ON [MentorshipProgressEntries]
END");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Notifications', 'Title') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Notifications_Title')
    BEGIN
        ALTER TABLE [Notifications] DROP CONSTRAINT [DF_Notifications_Title]
    END
    ALTER TABLE [Notifications] DROP COLUMN [Title]
END");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Notifications', 'Type') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Notifications_Type')
    BEGIN
        ALTER TABLE [Notifications] DROP CONSTRAINT [DF_Notifications_Type]
    END
    ALTER TABLE [Notifications] DROP COLUMN [Type]
END");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_MentorshipProgressEntries_StudentId_RoadmapStage'
      AND object_id = OBJECT_ID('MentorshipProgressEntries')
)
BEGIN
    CREATE UNIQUE INDEX [IX_MentorshipProgressEntries_StudentId_RoadmapStage]
    ON [MentorshipProgressEntries]([StudentId], [RoadmapStage])
END");
        }
    }
}
