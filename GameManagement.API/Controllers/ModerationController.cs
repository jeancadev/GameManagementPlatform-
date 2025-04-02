using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameManagement.Application.Interfaces;
using GameManagement.Application.DTOs;
using GameManagement.Application.DTOs.Moderation;

namespace GameManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ModerationController : ControllerBase
    {
        private readonly IModerationService _moderationService;
        private readonly ILogger<ModerationController> _logger;

        public ModerationController(
            IModerationService moderationService,
            ILogger<ModerationController> logger)
        {
            _moderationService = moderationService;
            _logger = logger;
        }

        [HttpPost("rooms/{roomId}/warn")]
        [Authorize(Roles = "Owner,Moderator")]
        public async Task<IActionResult> WarnPlayer(Guid roomId, [FromBody] WarnPlayerRequest request)
        {
            try
            {
                var moderatorId = Guid.Parse(User.FindFirst("sub")?.Value!);
                var logEntry = await _moderationService.WarnPlayerAsync(
                    roomId,
                    moderatorId,
                    request.PlayerId,
                    request.Reason
                );

                return Ok(new { message = "Advertencia enviada con éxito" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al advertir al jugador");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("rooms/{roomId}/mute")]
        [Authorize(Roles = "Owner,Moderator")]
        public async Task<IActionResult> MutePlayer(Guid roomId, [FromBody] MutePlayerRequest request)
        {
            try
            {
                var moderatorId = Guid.Parse(User.FindFirst("sub")?.Value!);
                var duration = TimeSpan.FromMinutes(request.DurationMinutes);
                var logEntry = await _moderationService.MutePlayerAsync(
                    roomId,
                    moderatorId,
                    request.PlayerId,
                    duration,
                    request.Reason
                );

                return Ok(new { message = "Jugador silenciado con éxito" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al silenciar al jugador");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("rooms/{roomId}/activity")]
        [Authorize(Roles = "Owner,Moderator")]
        public async Task<IActionResult> GetRoomActivity(Guid roomId)
        {
            try
            {
                var activity = await _moderationService.GetRoomActivityAsync(roomId);
                return Ok(activity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la actividad de la sala");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}