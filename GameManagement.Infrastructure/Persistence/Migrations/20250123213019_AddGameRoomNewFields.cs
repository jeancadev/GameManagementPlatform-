using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGameRoomNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRoomPlayers_Users_UserId",
                table: "GameRoomPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_GameRooms_Users_OwnerId",
                table: "GameRooms");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MaxWaitTimeToStart",
                table: "GameRooms",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "MinPlayersToStart",
                table: "GameRooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_GameRoomPlayers_Users_UserId",
                table: "GameRoomPlayers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameRooms_Users_OwnerId",
                table: "GameRooms",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRoomPlayers_Users_UserId",
                table: "GameRoomPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_GameRooms_Users_OwnerId",
                table: "GameRooms");

            migrationBuilder.DropColumn(
                name: "MaxWaitTimeToStart",
                table: "GameRooms");

            migrationBuilder.DropColumn(
                name: "MinPlayersToStart",
                table: "GameRooms");

            migrationBuilder.AddForeignKey(
                name: "FK_GameRoomPlayers_Users_UserId",
                table: "GameRoomPlayers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GameRooms_Users_OwnerId",
                table: "GameRooms",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
