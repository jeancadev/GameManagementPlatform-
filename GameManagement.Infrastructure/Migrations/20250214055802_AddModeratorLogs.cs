using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModeratorLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModeratorLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModeratorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModeratorLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModeratorLogs_ModeratorId",
                table: "ModeratorLogs",
                column: "ModeratorId");

            migrationBuilder.CreateIndex(
                name: "IX_ModeratorLogs_RoomId",
                table: "ModeratorLogs",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ModeratorLogs_TargetUserId",
                table: "ModeratorLogs",
                column: "TargetUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModeratorLogs");
        }
    }
}
