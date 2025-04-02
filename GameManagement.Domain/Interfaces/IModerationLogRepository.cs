using GameManagement.Domain.Entities;

public interface IModerationLogRepository
{
    Task CreateAsync(ModeratorLogEntry entry);
    Task<IEnumerable<ModeratorLogEntry>> GetRoomActivityAsync(Guid roomId);
    Task<IEnumerable<ModeratorLogEntry>> GetUserActivityAsync(Guid userId);
}