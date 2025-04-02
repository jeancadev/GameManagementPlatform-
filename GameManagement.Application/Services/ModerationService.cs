// GameManagement.Application/Services/ModerationService.cs
using GameManagement.Application.Interfaces;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Interfaces;
using GameManagement.Application.DTOs;
using Microsoft.Extensions.Logging;
using GameManagement.Domain.Notifications;
using GameManagement.Application.RealTime;

namespace GameManagement.Application.Services
{
    public class ModerationService : IModerationService
    {
        private readonly IModerationRepository _moderationRepository;
        private readonly IGameRoomRepository _gameRoomRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly IRealtimeNotificationService _realTimeService; 
        private readonly ILogger<ModerationService> _logger;

        public ModerationService(
            IModerationRepository moderationRepository,
            IGameRoomRepository gameRoomRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            ILogger<ModerationService> logger)
        {
            _moderationRepository = moderationRepository;
            _gameRoomRepository = gameRoomRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ModeratorLogEntry> WarnPlayerAsync(Guid roomId, Guid moderatorId, Guid targetUserId, string reason)
        {
            try
            {
                var targetUser = await _userRepository.GetByIdAsync(targetUserId);
                if (targetUser == null)
                    throw new InvalidOperationException("Usuario objetivo no encontrado");

                var logEntry = ModeratorLogEntry.Create(
                    roomId,
                    moderatorId,
                    "WARN",
                    $"Usuario advertido: {reason}",
                    targetUserId
                );

                await _moderationRepository.CreateLogEntryAsync(logEntry);

                // Notificación en tiempo real
                await _realTimeService.NotifyRoomUpdate(roomId.ToString(), new
                {
                    type = "MODERATION_WARN",
                    targetUserId = targetUserId.ToString(),
                    targetUsername = targetUser.Username,
                    reason = reason
                });

                // Notificación persistente
                await _notificationService.CreateNotificationAsync(
                    roomId,
                    NotificationType.PlayerWarned,
                    $"{targetUser.Username} ha sido advertido: {reason}",
                    moderatorId,
                    targetUserId
                );

                return logEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al advertir al jugador {TargetUserId}", targetUserId);
                throw;
            }
        }

        public async Task<ModeratorLogEntry> MutePlayerAsync(Guid roomId, Guid moderatorId, Guid targetUserId, TimeSpan duration, string reason)
        {
            try
            {
                var targetUser = await _userRepository.GetByIdAsync(targetUserId);
                if (targetUser == null)
                    throw new InvalidOperationException("Usuario objetivo no encontrado");

                var logEntry = ModeratorLogEntry.Create(
                    roomId,
                    moderatorId,
                    "MUTE",
                    $"Usuario silenciado por {duration.TotalMinutes} minutos: {reason}",
                    targetUserId
                );

                await _moderationRepository.CreateLogEntryAsync(logEntry);

                // Notificación en tiempo real
                await _realTimeService.NotifyRoomUpdate(roomId.ToString(), new
                {
                    type = "MODERATION_MUTE",
                    targetUserId = targetUserId.ToString(),
                    targetUsername = targetUser.Username,
                    duration = duration.TotalMinutes,
                    reason = reason
                });

                // Notificación persistente
                await _notificationService.CreateNotificationAsync(
                    roomId,
                    NotificationType.PlayerMuted,
                    $"{targetUser.Username} ha sido silenciado por {duration.TotalMinutes} minutos",
                    moderatorId,
                    targetUserId
                );

                return logEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al silenciar al jugador {TargetUserId}", targetUserId);
                throw;
            }
        }

        public async Task<ModeratorLogEntry> KickPlayerAsync(Guid roomId, Guid moderatorId, Guid targetUserId, string reason)
        {
            try
            {
                var targetUser = await _userRepository.GetByIdAsync(targetUserId);
                if (targetUser == null)
                    throw new InvalidOperationException("Usuario objetivo no encontrado");

                var logEntry = ModeratorLogEntry.Create(
                    roomId,
                    moderatorId,
                    "KICK",
                    $"Usuario expulsado: {reason}",
                    targetUserId
                );

                await _moderationRepository.CreateLogEntryAsync(logEntry);

                // Notificación en tiempo real
                await _realTimeService.NotifyRoomUpdate(roomId.ToString(), new
                {
                    type = "MODERATION_KICK",
                    targetUserId = targetUserId.ToString(),
                    targetUsername = targetUser.Username,
                    reason = reason
                });

                // Notificación persistente
                await _notificationService.CreateNotificationAsync(
                    roomId,
                    NotificationType.PlayerKicked,
                    $"{targetUser.Username} ha sido expulsado de la sala",
                    moderatorId,
                    targetUserId
                );

                return logEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al expulsar al jugador {TargetUserId}", targetUserId);
                throw;
            }
        }

        public async Task<IEnumerable<ModeratorLogEntry>> GetRoomActivityAsync(Guid roomId)
        {
            try
            {
                _logger.LogInformation("Obteniendo actividad de moderación para la sala {RoomId}", roomId);
                return await _moderationRepository.GetRoomActivityAsync(roomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividad de la sala {RoomId}", roomId);
                throw;
            }
        }

        public async Task<IEnumerable<ModeratorLogEntry>> GetUserActivityAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Obteniendo actividad de moderación para el usuario {UserId}", userId);
                return await _moderationRepository.GetUserActivityAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividad del usuario {UserId}", userId);
                throw;
            }
        }
    }
}