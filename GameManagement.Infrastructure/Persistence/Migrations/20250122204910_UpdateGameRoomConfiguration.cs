using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGameRoomConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRoom_Users_OwnerId",
                table: "GameRoom");

            migrationBuilder.DropForeignKey(
                name: "FK_GameRoomPlayers_GameRoom_GameRoomId",
                table: "GameRoomPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_GameRoomPlayers_Users_PlayersId",
                table: "GameRoomPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameRoom",
                table: "GameRoom");

            migrationBuilder.RenameTable(
                name: "GameRoom",
                newName: "GameRooms");

            migrationBuilder.RenameColumn(
                name: "PlayersId",
                table: "GameRoomPlayers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GameRoomPlayers_PlayersId",
                table: "GameRoomPlayers",
                newName: "IX_GameRoomPlayers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GameRoom_OwnerId",
                table: "GameRooms",
                newName: "IX_GameRooms_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameRooms",
                table: "GameRooms",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameRoomPlayers_GameRooms_GameRoomId",
                table: "GameRoomPlayers",
                column: "GameRoomId",
                principalTable: "GameRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRoomPlayers_GameRooms_GameRoomId",
                table: "GameRoomPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_GameRoomPlayers_Users_UserId",
                table: "GameRoomPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_GameRooms_Users_OwnerId",
                table: "GameRooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameRooms",
                table: "GameRooms");

            migrationBuilder.RenameTable(
                name: "GameRooms",
                newName: "GameRoom");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "GameRoomPlayers",
                newName: "PlayersId");

            migrationBuilder.RenameIndex(
                name: "IX_GameRoomPlayers_UserId",
                table: "GameRoomPlayers",
                newName: "IX_GameRoomPlayers_PlayersId");

            migrationBuilder.RenameIndex(
                name: "IX_GameRooms_OwnerId",
                table: "GameRoom",
                newName: "IX_GameRoom_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameRoom",
                table: "GameRoom",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameRoom_Users_OwnerId",
                table: "GameRoom",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GameRoomPlayers_GameRoom_GameRoomId",
                table: "GameRoomPlayers",
                column: "GameRoomId",
                principalTable: "GameRoom",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameRoomPlayers_Users_PlayersId",
                table: "GameRoomPlayers",
                column: "PlayersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
