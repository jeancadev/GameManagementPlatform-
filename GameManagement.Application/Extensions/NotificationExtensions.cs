using GameManagement.Application.DTOs;
using GameManagement.Domain.Entities;

namespace GameManagement.Application.Extensions
{
    public static class NotificationExtensions
    {
        public static NotificationResponse ToResponse(this Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                Type = notification.Type.ToString(),
                Message = notification.Message,
                RoomId = notification.RoomId,
                RoomName = notification.Room?.Name ?? string.Empty,
                SenderUsername = notification.Sender?.Username ?? string.Empty,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            };
        }

        public static IEnumerable<NotificationResponse> ToResponseList(
            this IEnumerable<Notification> notifications)
        {
            return notifications.Select(ToResponse);
        }
    }
}