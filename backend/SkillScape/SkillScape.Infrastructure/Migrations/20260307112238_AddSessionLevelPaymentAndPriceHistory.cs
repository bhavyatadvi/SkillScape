using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionLevelPaymentAndPriceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_Users_StudentId",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_StudentWallets_StudentId",
                table: "StudentWallets");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "MentorSessions");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "MentorSessions");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "MentorSessions");

            migrationBuilder.DropColumn(
                name: "PaymentTransactionId",
                table: "MentorSessions");

            migrationBuilder.DropColumn(
                name: "SessionPriceAtBooking",
                table: "MentorSessions");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "PaymentTransactions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "MentorSessionPriceHistory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MentorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SessionPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorSessionPriceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorSessionPriceHistory_Mentors_MentorId",
                        column: x => x.MentorId,
                        principalTable: "Mentors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentWallets_StudentId",
                table: "StudentWallets",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_SessionId",
                table: "PaymentTransactions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorSessionPriceHistory_MentorId_EffectiveFrom",
                table: "MentorSessionPriceHistory",
                columns: new[] { "MentorId", "EffectiveFrom" });

            migrationBuilder.Sql(@"
INSERT INTO [MentorSessionPriceHistory] ([Id], [MentorId], [SessionPrice], [EffectiveFrom], [CreatedAt])
SELECT CONVERT(nvarchar(450), NEWID()), [m].[Id],
       CASE WHEN COALESCE([m].[SessionPrice], [m].[HourlyRate], 0) > 0 THEN COALESCE([m].[SessionPrice], [m].[HourlyRate]) ELSE 500 END,
       COALESCE([m].[CreatedAt], GETUTCDATE()), GETUTCDATE()
FROM [Mentors] AS [m]
WHERE NOT EXISTS (
    SELECT 1
    FROM [MentorSessionPriceHistory] AS [h]
    WHERE [h].[MentorId] = [m].[Id]
);

DECLARE @VidhiId nvarchar(450) = (SELECT TOP(1) [Id] FROM [Users] WHERE [FullName] = N'Vidhi Trivedi');
DECLARE @EmilyUserId nvarchar(450) = (SELECT TOP(1) [Id] FROM [Users] WHERE [FullName] = N'Emily Watson');
DECLARE @EmilyMentorId nvarchar(450) = (SELECT TOP(1) [Id] FROM [Mentors] WHERE [UserId] = @EmilyUserId);
DECLARE @EnrollmentId nvarchar(450) = (
    SELECT TOP(1) [Id]
    FROM [MentorshipProgressEntries]
    WHERE [StudentId] = @VidhiId AND [MentorId] = @EmilyMentorId
);

IF @EnrollmentId IS NOT NULL
BEGIN
    DELETE FROM [PaymentTransactions] WHERE [MentorshipProgressId] = @EnrollmentId;

    UPDATE [MentorshipProgressEntries]
    SET [IsPaid] = 0,
        [PaidAt] = NULL,
        [Amount] = 0,
        [PaymentMethod] = NULL,
        [PaymentStatus] = N'Unpaid'
    WHERE [Id] = @EnrollmentId;

    INSERT INTO [PaymentTransactions] ([Id], [StudentId], [MentorshipProgressId], [SessionId], [Amount], [PaymentMethod], [Status], [CreatedAt], [CompletedAt])
    VALUES (CONVERT(nvarchar(450), NEWID()), @VidhiId, @EnrollmentId, NULL, 80, N'UPI', N'Completed', GETUTCDATE(), GETUTCDATE());

    UPDATE [MentorshipProgressEntries]
    SET [IsPaid] = 0,
        [PaidAt] = GETUTCDATE(),
        [Amount] = 80,
        [PaymentMethod] = N'UPI',
        [PaymentStatus] = N'Partial'
    WHERE [Id] = @EnrollmentId;

    IF EXISTS (SELECT 1 FROM [StudentWallets] WHERE [StudentId] = @VidhiId)
        UPDATE [StudentWallets] SET [Balance] = 920, [UpdatedAt] = GETUTCDATE() WHERE [StudentId] = @VidhiId;
    ELSE
        INSERT INTO [StudentWallets] ([Id], [StudentId], [Balance], [CreatedAt], [UpdatedAt])
        VALUES (CONVERT(nvarchar(450), NEWID()), @VidhiId, 920, GETUTCDATE(), GETUTCDATE());
END
");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_MentorSessions_SessionId",
                table: "PaymentTransactions",
                column: "SessionId",
                principalTable: "MentorSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_Users_StudentId",
                table: "PaymentTransactions",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_MentorSessions_SessionId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_Users_StudentId",
                table: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "MentorSessionPriceHistory");

            migrationBuilder.DropIndex(
                name: "IX_StudentWallets_StudentId",
                table: "StudentWallets");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_SessionId",
                table: "PaymentTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "PaymentTransactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "MentorSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "MentorSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "MentorSessions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentTransactionId",
                table: "MentorSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SessionPriceAtBooking",
                table: "MentorSessions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_StudentWallets_StudentId",
                table: "StudentWallets",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_Users_StudentId",
                table: "PaymentTransactions",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
