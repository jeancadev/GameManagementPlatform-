using GameManagement.Application.RealTime;
using GameManagement.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using GameManagement.API.Hubs;

namespace GameManagement.API.RealTime
{
    public class SignalRNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<GameHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IHubContext<GameHub> hubContext,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyRoomUpdate(string roomId, object update)
        {
            try
            {
                await _hubContext.Clients.Group(roomId)
                    .SendAsync("RoomUpdated", roomId, update);
                _logger.LogInformation("Notificación de actualización enviada a sala {RoomId}", roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de sala {RoomId}", roomId);
                throw;
            }
        }

        public async Task NotifyUser(string userId, string message)
        {
            try
            {
                await _hubContext.Clients.User(userId)
                    .SendAsync("UserNotification", message);
                _logger.LogInformation("Notificación enviada a usuario {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación a usuario {UserId}", userId);
                throw;
            }
        }

        public async Task BroadcastNotification(Notification notification)
        {
            try
            {
                await _hubContext.Clients.All
                    .SendAsync("Notification", notification);
                _logger.LogInformation("Notificación global enviada: {Message}", notification.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación global");
                throw;
            }
        }
    }
}