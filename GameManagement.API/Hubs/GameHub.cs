using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using GameManagement.Application.RealTime;

namespace GameManagement.API.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private readonly ILogger<GameHub> _logger;
        private readonly IRealtimeNotificationService _notificationService;

        public GameHub(
            ILogger<GameHub> logger,
            IRealtimeNotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            _logger.LogInformation("Usuario {Username} conectado. ID: {ConnectionId}",
                username, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public async Task JoinRoom(string roomId)
        {
            var username = Context.User?.Identity?.Name;
            _logger.LogInformation("Usuario {Username} uniéndose a sala {RoomId}",
                username, roomId);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await _notificationService.NotifyRoomUpdate(roomId, new
            {
                type = "JOIN",
                username = username,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task LeaveRoom(string roomId)
        {
            var username = Context.User?.Identity?.Name;
            _logger.LogInformation("Usuario {Username} abandonando sala {RoomId}",
                username, roomId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await _notificationService.NotifyRoomUpdate(roomId, new
            {
                type = "LEAVE",
                username = username,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyModeration(string roomId, string action, string targetUsername)
        {
            try
            {
                var username = Context.User?.Identity?.Name;
                _logger.LogInformation("Acción de moderación: {Action} por {Username} en sala {RoomId}",
                    action, username, roomId);

                await _notificationService.NotifyRoomUpdate(roomId, new
                {
                    type = "MODERATION",
                    action = action,
                    moderator = username,
                    target = targetUsername,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al notificar acción de moderación");
                throw;
            }
        }
    }
}