using GameManagement.Domain.Entities;

namespace GameManagement.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
        Task<IEnumerable<Notification>> GetRoomNotificationsAsync(Guid roomId);
        Task CreateAsync(Notification notification);
        Task MarkAsReadAsync(Guid notificationId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId);
        Task<int> GetTotalNotificationsCountAsync();
    }
}