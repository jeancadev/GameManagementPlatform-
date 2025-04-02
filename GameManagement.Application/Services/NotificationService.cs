using GameManagement.Application.DTOs;
using GameManagement.Application.Extensions;
using GameManagement.Application.Interfaces;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Interfaces;
using GameManagement.Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace GameManagement.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Notification> CreateNotificationAsync(
            Guid roomId,
            NotificationType type,
            string message,
            Guid? senderId = null,
            Guid? receiverId = null)
                {
            try
            {
                _logger.LogInformation("Iniciando creación de notificación: {Type} para sala {RoomId}", type, roomId);
                var notification = Notification.Create(roomId, type, message, senderId, receiverId);
                await _notificationRepository.CreateAsync(notification);
                _logger.LogInformation("Notificación creada con ID {NotificationId}", notification.Id);
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear notificación: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Obteniendo notificaciones para usuario {UserId}", userId);
                var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
                var responses = notifications.Select(n => new NotificationResponse
                {
                    Id = n.Id,
                    Type = n.Type.ToString(),
                    Message = n.Message,
                    RoomId = n.RoomId,
                    RoomName = n.Room?.Name ?? string.Empty,
                    SenderUsername = n.Sender?.Username ?? string.Empty,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead
                });
                _logger.LogInformation("Se encontraron {Count} notificaciones para el usuario", notifications.Count());
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones del usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<NotificationResponse>> GetRoomNotificationsAsync(Guid roomId)
        {
            try
            {
                _logger.LogInformation("Obteniendo notificaciones para sala {RoomId}", roomId);
                var notifications = await _notificationRepository.GetRoomNotificationsAsync(roomId);
                var responses = notifications.Select(n => new NotificationResponse
                {
                    Id = n.Id,
                    Type = n.Type.ToString(),
                    Message = n.Message,
                    RoomId = n.RoomId,
                    RoomName = n.Room?.Name ?? string.Empty,
                    SenderUsername = n.Sender?.Username ?? string.Empty,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead
                });
                _logger.LogInformation("Se encontraron {Count} notificaciones para la sala", notifications.Count());
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones de la sala {RoomId}", roomId);
                throw;
            }
        }

        public async Task<IEnumerable<NotificationResponse>> GetUnreadNotificationsAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Obteniendo notificaciones no leídas para usuario {UserId}", userId);
                var notifications = await _notificationRepository.GetUnreadNotificationsAsync(userId);
                var responses = notifications.Select(n => new NotificationResponse
                {
                    Id = n.Id,
                    Type = n.Type.ToString(),
                    Message = n.Message,
                    RoomId = n.RoomId,
                    RoomName = n.Room?.Name ?? string.Empty,
                    SenderUsername = n.Sender?.Username ?? string.Empty,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead
                });
                _logger.LogInformation("Se encontraron {Count} notificaciones no leídas", notifications.Count());
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones no leídas del usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetTotalNotificationsCountAsync()
        {
            try
            {
                return await _notificationRepository.GetTotalNotificationsCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el conteo total de notificaciones");
                throw;
            }
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                _logger.LogInformation("Marcando notificación {NotificationId} como leída", notificationId);
                await _notificationRepository.MarkAsReadAsync(notificationId);
                _logger.LogInformation("Notificación marcada como leída exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar notificación como leída {NotificationId}", notificationId);
                throw;
            }
        }
    }
}