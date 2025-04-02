using GameManagement.Domain.Entities;

namespace GameManagement.Application.RealTime
{
    public interface IRealtimeNotificationService
    {
        Task NotifyRoomUpdate(string roomId, object update);
        Task NotifyUser(string userId, string message);
        Task BroadcastNotification(Notification notification);
    }
}