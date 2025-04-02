using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGameRoomRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameRoomPlayers");

            migrationBuilder.AddColumn<Guid>(
                name: "GameRoomId1",
                table: "UserGameRooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGameRooms_GameRoomId1",
                table: "UserGameRooms",
                column: "GameRoomId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserGameRooms_GameRooms_GameRoomId1",
                table: "UserGameRooms",
                column: "GameRoomId1",
                principalTable: "GameRooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGameRooms_GameRooms_GameRoomId1",
                table: "UserGameRooms");

            migrationBuilder.DropIndex(
                name: "IX_UserGameRooms_GameRoomId1",
                table: "UserGameRooms");

            migrationBuilder.DropColumn(
                name: "GameRoomId1",
                table: "UserGameRooms");

            migrationBuilder.CreateTable(
                name: "GameRoomPlayers",
                columns: table => new
                {
                    GameRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRoomPlayers", x => new { x.GameRoomId, x.UserId });
                    table.ForeignKey(
                        name: "FK_GameRoomPlayers_GameRooms_GameRoomId",
                        column: x => x.GameRoomId,
                        principalTable: "GameRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameRoomPlayers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameRoomPlayers_UserId",
                table: "GameRoomPlayers",
                column: "UserId");
        }
    }
}
