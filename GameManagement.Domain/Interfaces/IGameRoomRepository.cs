using GameManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace GameManagement.Domain.Interfaces
{
    public interface IGameRoomRepository
    {
        Task<GameRoom> GetByIdAsync(Guid id);
        Task<IEnumerable<GameRoom>> GetAvailableRoomsAsync();
        Task CreateAsync(GameRoom gameRoom);
        Task UpdateAsync(GameRoom gameRoom);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name);
        Task<IEnumerable<GameRoom>> GetActiveRoomsByUserIdAsync(Guid userId);
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}