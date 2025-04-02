using Microsoft.EntityFrameworkCore;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Interfaces;
using GameManagement.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace GameManagement.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly GameManagementDbContext _context;
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(
            GameManagementDbContext context,
            ILogger<NotificationRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Buscando notificaciones en BD para usuario {UserId}", userId);
                var notifications = await _context.Notifications
                    .Include(n => n.Sender)
                    .Include(n => n.Room)
                    .Where(n =>
                        n.ReceiverId == userId || // Notificaciones directas al usuario
                        (n.Room != null && n.Room.UserRooms.Any(ur => ur.UserId == userId)) // Notificaciones de salas donde está el usuario
                    )
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Se encontraron {Count} notificaciones", notifications.Count);
                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones de BD");
                throw;
            }
        }

        public async Task<int> GetTotalNotificationsCountAsync()
        {
            try
            {
                var count = await _context.Notifications.CountAsync();
                _logger.LogInformation("Total de notificaciones en BD: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar notificaciones");
                throw;
            }
        }

        public async Task<IEnumerable<Notification>> GetRoomNotificationsAsync(Guid roomId)
        {
            try
            {
                return await _context.Notifications
                    .Include(n => n.Sender)
                    .Include(n => n.Receiver)
                    .Where(n => n.RoomId == roomId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones de la sala {RoomId}", roomId);
                throw;
            }
        }

        public async Task CreateAsync(Notification notification)
        {
            try
            {
                _logger.LogInformation("Creando notificación para sala {RoomId}", notification.RoomId);
                await _context.Notifications.AddAsync(notification);

                var entries = _context.ChangeTracker.Entries()
                    .Where(e => e.State != EntityState.Unchanged)
                    .ToList();

                foreach (var entry in entries)
                {
                    _logger.LogInformation("Cambio: {EntityType} - Estado: {State}",
                        entry.Entity.GetType().Name, entry.State);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error detallado al crear notificación: {Message}",
                    ex.InnerException?.Message);
                // Sin propagar el error para que no interrumpa el flujo principal
            }
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.MarkAsRead();
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Notificación marcada como leída: {NotificationId}", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar notificación como leída: {NotificationId}", notificationId);
                throw;
            }
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Buscando notificaciones no leídas para usuario {UserId}", userId);
                var notifications = await _context.Notifications
                    .Include(n => n.Sender)
                    .Include(n => n.Room)
                    .Where(n =>
                        !n.IsRead &&
                        (n.ReceiverId == userId ||
                        (n.Room != null && n.Room.UserRooms.Any(ur => ur.UserId == userId)))
                    )
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Se encontraron {Count} notificaciones no leídas", notifications.Count);
                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones no leídas");
                throw;
            }
        }
    }
}