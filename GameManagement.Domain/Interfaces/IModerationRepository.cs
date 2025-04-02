using GameManagement.Domain.Entities;

public interface IModerationRepository
{
    Task<IEnumerable<ModeratorLogEntry>> GetRoomActivityAsync(Guid roomId);
    Task<ModeratorLogEntry> CreateLogEntryAsync(ModeratorLogEntry entry);
    Task<IEnumerable<ModeratorLogEntry>> GetUserActivityAsync(Guid userId);
    Task ClearOldLogsAsync(DateTime olderThan);
}