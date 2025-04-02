using GameManagement.Application.DTOs;

namespace GameManagement.Application.Interfaces
{
    public interface IGameRoomService
    {
        Task<GameRoomResponse> CreateGameRoomAsync(Guid userId, CreateGameRoomRequest request);
        Task<GameRoomResponse> GetGameRoomByIdAsync(Guid roomId);
        Task<IEnumerable<GameRoomResponse>> GetAvailableGameRoomsAsync();
        Task<GameRoomResponse> JoinGameRoomAsync(Guid userId, Guid roomId);
        Task<GameRoomResponse> LeaveGameRoomAsync(Guid userId, Guid roomId);
        Task<GameRoomResponse> StartGameAsync(Guid userId, Guid roomId);
        Task<GameRoomResponse> EndGameAsync(Guid userId, Guid roomId);
        Task<GameRoomResponse> KickPlayerAsync(Guid requestingUserId, Guid roomId, Guid targetUserId);
        Task<GameRoomResponse> TransferOwnershipAsync(Guid currentOwnerId, Guid roomId, Guid newOwnerId);
    }
}