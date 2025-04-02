using GameManagement.Application.DTOs;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Notifications;

namespace GameManagement.Application.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(
            Guid roomId,
            NotificationType type,
            string message,
            Guid? senderId = null,
            Guid? receiverId = null);

        Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId);
        Task<IEnumerable<NotificationResponse>> GetRoomNotificationsAsync(Guid roomId);
        Task<IEnumerable<NotificationResponse>> GetUnreadNotificationsAsync(Guid userId);
        Task<int> GetTotalNotificationsCountAsync();
        Task MarkAsReadAsync(Guid notificationId);
    }
}