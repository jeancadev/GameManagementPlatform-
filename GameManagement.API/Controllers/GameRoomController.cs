using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Common;
using System.Security.Claims;
using GameManagement.Domain.Interfaces;
using GameManagement.Domain.Enums;

namespace GameManagement.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class GameRoomController : ControllerBase
    {
        private readonly IGameRoomService _gameRoomService;
        private readonly IGameRoomRepository _gameRoomRepository;
        private readonly ILogger<GameRoomController> _logger;

        public GameRoomController(
            IGameRoomService gameRoomService,
            IGameRoomRepository gameRoomRepository,
            ILogger<GameRoomController> logger)
        {
            _gameRoomService = gameRoomService;
            _gameRoomRepository = gameRoomRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<GameRoomResponse>>> CreateRoom([FromBody] CreateGameRoomRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de sala de juego");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido en la creación de sala");
                    return BadRequest(ApiResponse<GameRoomResponse>.ErrorResponse("Datos de sala inválidos"));
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogError("No se pudo obtener el ID del usuario del token");
                    return Unauthorized(ApiResponse<GameRoomResponse>.ErrorResponse("Usuario no autorizado"));
                }

                var userId = Guid.Parse(userIdClaim.Value);
                _logger.LogInformation("Creando sala para usuario {UserId}", userId);

                var result = await _gameRoomService.CreateGameRoomAsync(userId, request);
                return Ok(ApiResponse<GameRoomResponse>.SuccessResponse(result, "Sala creada exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detallado al crear sala de juego: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<GameRoomResponse>.ErrorResponse($"Error al crear la sala de juego: {ex.Message}"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GameRoomResponse>>>> GetAvailableRooms()
        {
            try
            {
                var rooms = await _gameRoomService.GetAvailableGameRoomsAsync();
                return Ok(ApiResponse<IEnumerable<GameRoomResponse>>.SuccessResponse(rooms, "Salas disponibles recuperadas exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener salas disponibles");
                return StatusCode(500, ApiResponse<IEnumerable<GameRoomResponse>>.ErrorResponse("Error al obtener las salas disponibles"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GameRoomResponse>>> GetRoomById(Guid id)
        {
            try
            {
                var room = await _gameRoomService.GetGameRoomByIdAsync(id);
                return Ok(ApiResponse<GameRoomResponse>.SuccessResponse(room, "Sala recuperada exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sala {RoomId}", id);
                return StatusCode(500, ApiResponse<GameRoomResponse>.ErrorResponse("Error al obtener la sala"));
            }
        }

        [HttpGet("diagnostic/user-rooms")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserRoomsDiagnostic()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var activeRooms = await _gameRoomRepository.GetActiveRoomsByUserIdAsync(userId);
                var roomsList = activeRooms.Select(r => new
                {
                    RoomId = r.Id,
                    RoomName = r.Name,
                    IsOwner = r.UserRooms.Any(ur => ur.UserId == userId && ur.Role == PlayerRole.Owner),
                    IsPlayer = r.UserRooms.Any(ur => ur.UserId == userId),
                    PlayerCount = r.UserRooms.Count,
                    Players = r.UserRooms.Select(ur => new
                    {
                        Username = ur.User.Username,
                        Role = ur.Role.ToString()
                    })
                }).ToList();

                return Ok(ApiResponse<object>.SuccessResponse(new
                {
                    UserId = userId,
                    ActiveRooms = roomsList
                }, "Diagnóstico de salas del usuario"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en diagnóstico de salas del usuario");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{roomId}/join")]
        public async Task<ActionResult<ApiResponse<GameRoomResponse>>> JoinRoom(Guid roomId)
        {
            try
            {
                _logger.LogInformation("Iniciando solicitud de unión a sala. RoomId: {RoomId}", roomId);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("ID de usuario no encontrado en el token");
                    return BadRequest(ApiResponse<GameRoomResponse>.ErrorResponse("Usuario no autorizado"));
                }

                _logger.LogInformation("Usuario {UserId} intentando unirse a sala {RoomId}", userId, roomId);
                var result = await _gameRoomService.JoinGameRoomAsync(userId, roomId);

                _logger.LogInformation("Usuario {UserId} se unió exitosamente a sala {RoomId}", userId, roomId);
                return Ok(ApiResponse<GameRoomResponse>.SuccessResponse(result, "Te has unido a la sala exitosamente"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error controlado al unirse a la sala: {Message}", ex.Message);
                return BadRequest(ApiResponse<GameRoomResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al unirse a la sala: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<GameRoomResponse>.ErrorResponse($"Error al unirse a la sala: {ex.Message}"));
            }
        }

        [HttpPost("{roomId}/leave")]
        public async Task<ActionResult<ApiResponse<GameRoomResponse>>> LeaveRoom(Guid roomId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _gameRoomService.LeaveGameRoomAsync(userId, roomId);
                return Ok(ApiResponse<GameRoomResponse>.SuccessResponse(result, "Has abandonado la sala exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abandonar la sala {RoomId}", roomId);
                return StatusCode(500, ApiResponse<GameRoomResponse>.ErrorResponse("Error al abandonar la sala"));
            }
        }

        [HttpPost("{roomId}/kick/{targetUserId}")]
        public async Task<ActionResult<ApiResponse<GameRoomResponse>>> KickPlayer(Guid roomId, Guid targetUserId)
        {
            try
            {
                var requestingUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _gameRoomService.KickPlayerAsync(requestingUserId, roomId, targetUserId);
                return Ok(ApiResponse<GameRoomResponse>.SuccessResponse(result, "Jugador expulsado exitosamente"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al expulsar jugador: {Message}", ex.Message);
                return BadRequest(ApiResponse<GameRoomResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al expulsar jugador");
                return StatusCode(500, ApiResponse<GameRoomResponse>.ErrorResponse("Error al expulsar al jugador"));
            }
        }

        [HttpPost("{roomId}/transfer-ownership/{newOwnerId}")]
        public async Task<ActionResult<ApiResponse<GameRoomResponse>>> TransferOwnership(Guid roomId, Guid newOwnerId)
        {
            try
            {
                var currentOwnerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _gameRoomService.TransferOwnershipAsync(currentOwnerId, roomId, newOwnerId);
                return Ok(ApiResponse<GameRoomResponse>.SuccessResponse(result, "Propiedad transferida exitosamente"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al transferir propiedad: {Message}", ex.Message);
                return BadRequest(ApiResponse<GameRoomResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al transferir propiedad");
                return StatusCode(500, ApiResponse<GameRoomResponse>.ErrorResponse("Error al transferir la propiedad"));
            }
        }

        [HttpPost("{roomId}/start")]
        public async Task<ActionResult<ApiResponse<GameRoomResponse>>> StartGame(Guid roomId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _gameRoomService.StartGameAsync(userId, roomId);
                return Ok(ApiResponse<GameRoomResponse>.SuccessResponse(result, "Juego iniciado exitosamente"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al iniciar el juego: {Message}", ex.Message);
                return BadRequest(ApiResponse<GameRoomResponse>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al iniciar el juego");
                return StatusCode(500, ApiResponse<GameRoomResponse>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{roomId}/end")]
        public async Task<ActionResult<ApiResponse<GameRoomResponse>>> EndGame(Guid roomId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _gameRoomService.EndGameAsync(userId, roomId);
                return Ok(ApiResponse<GameRoomResponse>.SuccessResponse(result, "Juego finalizado exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar el juego en la sala {RoomId}", roomId);
                return StatusCode(500, ApiResponse<GameRoomResponse>.ErrorResponse("Error al finalizar el juego"));
            }
        }
    }
}