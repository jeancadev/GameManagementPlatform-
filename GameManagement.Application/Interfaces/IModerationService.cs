using GameManagement.Domain.Entities;

namespace GameManagement.Application.Interfaces
{
    public interface IModerationService
    {
        Task<ModeratorLogEntry> WarnPlayerAsync(Guid roomId, Guid moderatorId, Guid targetUserId, string reason);
        Task<ModeratorLogEntry> MutePlayerAsync(Guid roomId, Guid moderatorId, Guid targetUserId, TimeSpan duration, string reason);
        Task<ModeratorLogEntry> KickPlayerAsync(Guid roomId, Guid moderatorId, Guid targetUserId, string reason);
        Task<IEnumerable<ModeratorLogEntry>> GetRoomActivityAsync(Guid roomId);
        Task<IEnumerable<ModeratorLogEntry>> GetUserActivityAsync(Guid userId);
    }
}