using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPeerStudyRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_MentorSessions_SessionId",
                table: "PaymentTransactions");

            migrationBuilder.CreateTable(
                name: "PeerStudyRooms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CollaborationType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    MaxMembers = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    SharedNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    WhiteboardState = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerStudyRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeerStudyRooms_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeerStudyRoomMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerStudyRoomMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeerStudyRoomMessages_PeerStudyRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "PeerStudyRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerStudyRoomMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeerStudyRoomParticipants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerStudyRoomParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeerStudyRoomParticipants_PeerStudyRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "PeerStudyRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerStudyRoomParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeerStudyRoomTasks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerStudyRoomTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeerStudyRoomTasks_PeerStudyRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "PeerStudyRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerStudyRoomTasks_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeerStudyRoomMessages_RoomId",
                table: "PeerStudyRoomMessages",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerStudyRoomMessages_SenderId",
                table: "PeerStudyRoomMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerStudyRoomParticipants_RoomId_UserId",
                table: "PeerStudyRoomParticipants",
                columns: new[] { "RoomId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeerStudyRoomParticipants_UserId",
                table: "PeerStudyRoomParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerStudyRooms_CreatorId",
                table: "PeerStudyRooms",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerStudyRoomTasks_AssignedToUserId",
                table: "PeerStudyRoomTasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerStudyRoomTasks_RoomId",
                table: "PeerStudyRoomTasks",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_MentorSessions_SessionId",
                table: "PaymentTransactions",
                column: "SessionId",
                principalTable: "MentorSessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_MentorSessions_SessionId",
                table: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "PeerStudyRoomMessages");

            migrationBuilder.DropTable(
                name: "PeerStudyRoomParticipants");

            migrationBuilder.DropTable(
                name: "PeerStudyRoomTasks");

            migrationBuilder.DropTable(
                name: "PeerStudyRooms");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_MentorSessions_SessionId",
                table: "PaymentTransactions",
                column: "SessionId",
                principalTable: "MentorSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
