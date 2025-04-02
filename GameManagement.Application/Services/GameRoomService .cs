using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using GameManagement.Domain.Enums;
using GameManagement.Domain.Notifications;
using Microsoft.EntityFrameworkCore.Storage;
using GameManagement.Application.RealTime;

namespace GameManagement.Application.Services
{
    public class GameRoomService : IGameRoomService
    {
        private readonly INotificationService _notificationService;
        private readonly IGameRoomRepository _gameRoomRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GameRoomService> _logger;
        private readonly IRealtimeNotificationService _realTimeService;

        public GameRoomService(
            IGameRoomRepository gameRoomRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            IRealtimeNotificationService realTimeService,
            ILogger<GameRoomService> logger)
        {
            _gameRoomRepository = gameRoomRepository ?? throw new ArgumentNullException(nameof(gameRoomRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _notificationService = notificationService;
            _realTimeService = realTimeService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GameRoomResponse> KickPlayerAsync(Guid requestingUserId, Guid roomId, Guid targetUserId)
        {
            _logger.LogInformation(
                "Usuario {RequestingUserId} intentando expulsar a usuario {TargetUserId} de la sala {RoomId}",
                requestingUserId, targetUserId, roomId);

            var gameRoom = await ValidateAndGetGameRoom(roomId);
            var targetUser = await ValidateAndGetUser(targetUserId);

            try
            {
                gameRoom.KickPlayer(targetUserId, requestingUserId);
                await SendNotificationAndSaveChanges(
                    gameRoom,
                    NotificationType.PlayerKicked,
                    $"{targetUser.Username} ha sido expulsado de la sala",
                    requestingUserId,
                    targetUserId);

                return await MapToResponseAsync(gameRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al expulsar usuario de la sala");
                throw;
            }
        }

        public async Task<GameRoomResponse> TransferOwnershipAsync(Guid currentOwnerId, Guid roomId, Guid newOwnerId)
        {
            _logger.LogInformation(
                "Transfiriendo propiedad de sala {RoomId} de usuario {CurrentOwnerId} a usuario {NewOwnerId}",
                roomId, currentOwnerId, newOwnerId);

            var gameRoom = await ValidateAndGetGameRoom(roomId);

            try
            {
                gameRoom.TransferOwnership(newOwnerId, currentOwnerId);
                await _gameRoomRepository.UpdateAsync(gameRoom);

                _logger.LogInformation(
                    "Propiedad de sala {RoomId} transferida exitosamente a usuario {NewOwnerId}",
                    roomId, newOwnerId);

                return await MapToResponseAsync(gameRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al transferir propiedad de la sala");
                throw;
            }
        }

        public async Task<GameRoomResponse> CreateGameRoomAsync(Guid userId, CreateGameRoomRequest request)
        {
            _logger.LogInformation("Creando nueva sala de juego para el usuario {UserId}", userId);
            var user = await ValidateAndGetUser(userId);
            await ValidateRoomCreation(request);

            try
            {
                var gameRoom = GameRoom.Create(
                    request.Name,
                    request.Description,
                    request.MaxPlayers,
                    userId);

                gameRoom.AddPlayer(user, PlayerRole.Owner);
                await _gameRoomRepository.CreateAsync(gameRoom);
                await SendNotificationAndSaveChanges(
                    gameRoom,
                    NotificationType.RoomCreated,
                    $"Nueva sala creada: {gameRoom.Name}");

                return await MapToResponseAsync(gameRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear sala para usuario {UserId}", userId);
                throw new InvalidOperationException($"Error al crear la sala de juego: {ex.Message}");
            }
        }

        public async Task<GameRoomResponse> GetGameRoomByIdAsync(Guid roomId)
        {
            var gameRoom = await ValidateAndGetGameRoom(roomId);
            return await MapToResponseAsync(gameRoom);
        }

        public async Task<IEnumerable<GameRoomResponse>> GetAvailableGameRoomsAsync()
        {
            _logger.LogInformation("Obteniendo salas de juego disponibles");
            var rooms = await _gameRoomRepository.GetAvailableRoomsAsync();
            return await Task.WhenAll(rooms.Select(MapToResponseAsync));
        }

        public async Task<GameRoomResponse> JoinGameRoomAsync(Guid userId, Guid roomId)
        {
            _logger.LogInformation("Usuario {UserId} intentando unirse a sala {GameRoomId}", userId, roomId);
            var user = await ValidateAndGetUser(userId);
            var gameRoom = await ValidateAndGetGameRoom(roomId);
            await ValidateUserJoinRoom(userId, roomId);

            try
            {
                gameRoom.AddPlayer(user, PlayerRole.Player);
                await SendNotificationAndSaveChanges(
                    gameRoom,
                    NotificationType.PlayerJoined,
                    $"{user.Username} se ha unido a la sala",
                    userId);

                // Crear y enviar notificación
                var notification = await _notificationService.CreateNotificationAsync(
                    roomId,
                    NotificationType.PlayerJoined,
                    $"{user.Username} se ha unido a la sala",
                    userId);

                // Enviar actualización en tiempo real
                await _realTimeService.NotifyRoomUpdate(roomId.ToString(), new
                {
                    type = "JOIN",
                    userId = userId.ToString(),
                    username = user.Username,
                    currentPlayers = gameRoom.UserRooms.Count
                });

                await _realTimeService.BroadcastNotification(notification);

                return await MapToResponseAsync(gameRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al unir usuario a sala: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<GameRoomResponse> LeaveGameRoomAsync(Guid userId, Guid roomId)
        {
            _logger.LogInformation("Iniciando proceso de abandono de sala. UserId: {UserId}, RoomId: {RoomId}", userId, roomId);

            var user = await ValidateAndGetUser(userId);
            var gameRoom = await ValidateAndGetGameRoom(roomId);

            try
            {
                gameRoom.RemovePlayer(user);
                await _gameRoomRepository.UpdateAsync(gameRoom);
                _logger.LogInformation("Usuario removido exitosamente de la sala");

                await _notificationService.CreateNotificationAsync(
                roomId,
                NotificationType.PlayerLeft,
                $"{user.Username} ha abandonado la sala",
                userId);

                return await MapToResponseAsync(gameRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al remover usuario de la sala: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<GameRoomResponse> StartGameAsync(Guid userId, Guid roomId)
        {
            var gameRoom = await ValidateAndGetGameRoom(roomId);
            await ValidateGameOwnership(userId, gameRoom, "iniciar");

            try
            {
                gameRoom.Start();
                await SendNotificationAndSaveChanges(
                    gameRoom,
                    NotificationType.GameStarted,
                    "La partida ha comenzado",
                    userId);

                return await MapToResponseAsync(gameRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar la sala: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<GameRoomResponse> EndGameAsync(Guid userId, Guid roomId)
        {
            var gameRoom = await ValidateAndGetGameRoom(roomId);
            await ValidateGameOwnership(userId, gameRoom, "finalizar");

            try
            {
                gameRoom.End();
                await SendNotificationAndSaveChanges(
                    gameRoom,
                    NotificationType.GameEnded,
                    "La partida ha finalizado",
                    userId);

                return await MapToResponseAsync(gameRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar la sala: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<User> ValidateAndGetUser(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado. ID: {UserId}", userId);
                throw new InvalidOperationException("Usuario no encontrado");
            }
            return user;
        }

        private async Task<GameRoom> ValidateAndGetGameRoom(Guid roomId)
        {
            var gameRoom = await _gameRoomRepository.GetByIdAsync(roomId);
            if (gameRoom == null)
            {
                _logger.LogWarning("Sala no encontrada. ID: {RoomId}", roomId);
                throw new InvalidOperationException("Sala de juego no encontrada");
            }
            return gameRoom;
        }

        private async Task ValidateRoomCreation(CreateGameRoomRequest request)
        {
            if (await _gameRoomRepository.ExistsByNameAsync(request.Name))
            {
                _logger.LogWarning("Intento de crear sala con nombre duplicado: {Name}", request.Name);
                throw new InvalidOperationException("Ya existe una sala con este nombre");
            }
        }

        private async Task ValidateUserJoinRoom(Guid userId, Guid roomId)
        {
            var activeRooms = await _gameRoomRepository.GetActiveRoomsByUserIdAsync(userId);
            var userActiveRooms = activeRooms.ToList();

            var isAlreadyInOtherRoom = userActiveRooms
                .Where(r => r.Id != roomId) // Excluyo la sala actual
                .Any(r => r.UserRooms.Any(ur => ur.UserId == userId && ur.Role != PlayerRole.Owner));

            if (isAlreadyInOtherRoom)
            {
                throw new InvalidOperationException("No puedes unirte a múltiples salas activas simultáneamente");
            }
        }

        private async Task SendNotificationAndSaveChanges(GameRoom gameRoom, NotificationType type, string message, Guid? senderId = null, Guid? receiverId = null)
        {
            using var transaction = await _gameRoomRepository.BeginTransactionAsync();
            try
            {
                await _gameRoomRepository.UpdateAsync(gameRoom);
                await _notificationService.CreateNotificationAsync(gameRoom.Id, type, message, senderId, receiverId);
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Transacción completada: actualización de sala y creación de notificación para {Type}",
                    type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en transacción de sala/notificación");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<GameRoomResponse> UpdatePlayerRoleAsync(Guid requestingUserId, Guid roomId, Guid targetUserId, PlayerRole newRole)
        {
            var gameRoom = await ValidateAndGetGameRoom(roomId);
            var targetUser = await ValidateAndGetUser(targetUserId);
            await ValidateGameOwnership(requestingUserId, gameRoom, "modificar roles");

            try
            {
                gameRoom.UpdatePlayerRole(targetUserId, newRole, requestingUserId);
                await _gameRoomRepository.UpdateAsync(gameRoom);

                _logger.LogInformation("Rol actualizado exitosamente para usuario {UserId} en sala {RoomId}",
                    targetUserId, roomId);

                // Crear notificación en la base de datos
                var notification = await _notificationService.CreateNotificationAsync(
                    roomId,
                    NotificationType.RoleChanged,
                    $"El rol de {targetUser.Username} ha sido actualizado a {newRole}",
                    requestingUserId,
                    targetUserId
                );

                // Enviar actualización en tiempo real
                await _realTimeService.NotifyRoomUpdate(roomId.ToString(), new
                {
                    type = "ROLE_CHANGE",
                    targetUserId = targetUserId.ToString(),
                    targetUsername = targetUser.Username,
                    newRole = newRole.ToString(),
                    updatedBy = requestingUserId.ToString()
                });

                // Enviar notificación en tiempo real
                await _realTimeService.BroadcastNotification(notification);

                return await MapToResponseAsync(gameRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol de usuario");
                throw;
            }
        }

        private async Task ValidateGameOwnership(Guid userId, GameRoom gameRoom, string action)
        {
            var userRoom = gameRoom.UserRooms.FirstOrDefault(ur => ur.UserId == userId);
            if (userRoom?.Role != PlayerRole.Owner)
            {
                _logger.LogWarning("Usuario {UserId} no autorizado para {Action} la sala {GameRoomId}",
                    userId, action, gameRoom.Id);
                throw new InvalidOperationException($"Solo el creador de la sala puede {action} el juego");
            }
        }

        private async Task<GameRoomResponse> MapToResponseAsync(GameRoom gameRoom)
        {
            var players = gameRoom.UserRooms.Select(ur => new PlayerInfo
            {
                Username = ur.User.Username,
                Role = ur.Role.ToString()
            }).ToList();

            return new GameRoomResponse
            {
                Id = gameRoom.Id,
                Name = gameRoom.Name,
                Description = gameRoom.Description,
                MaxPlayers = gameRoom.MaxPlayers,
                CurrentPlayers = gameRoom.UserRooms.Count,
                Status = gameRoom.Status.ToString(),
                OwnerUsername = gameRoom.UserRooms
                    .FirstOrDefault(ur => ur.Role == PlayerRole.Owner)?.User.Username,
                CreatedAt = gameRoom.CreatedAt,
                StartedAt = gameRoom.StartedAt,
                Players = players
            };
        }
    }   
    public class PlayerInfo
    {
        public string Username { get; set; }
        public string Role { get; set; }
    }
}