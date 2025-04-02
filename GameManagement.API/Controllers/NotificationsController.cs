using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Common;
using System.Security.Claims;

namespace GameManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet("debug/count")]
        public async Task<ActionResult<ApiResponse<int>>> GetTotalCount()
        {
            var count = await _notificationService.GetTotalNotificationsCountAsync();
            return Ok(ApiResponse<int>.SuccessResponse(count, "Conteo total de notificaciones"));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<NotificationResponse>>>> GetUserNotifications()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);

                return Ok(ApiResponse<IEnumerable<NotificationResponse>>.SuccessResponse(
                    notifications,
                    "Notificaciones recuperadas exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones");
                return StatusCode(500, ApiResponse<IEnumerable<NotificationResponse>>.ErrorResponse(
                    "Error al obtener notificaciones"));
            }
        }

        [HttpGet("unread")]
        public async Task<ActionResult<ApiResponse<IEnumerable<NotificationResponse>>>> GetUnreadNotifications()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);

                return Ok(ApiResponse<IEnumerable<NotificationResponse>>.SuccessResponse(
                    notifications,
                    "Notificaciones no leídas recuperadas exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones no leídas");
                return StatusCode(500, ApiResponse<IEnumerable<NotificationResponse>>.ErrorResponse(
                    "Error al obtener notificaciones no leídas"));
            }
        }

        [HttpPost("{notificationId}/mark-as-read")]
        public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(Guid notificationId)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(notificationId);
                return Ok(ApiResponse<object>.SuccessResponse(
                    null,
                    "Notificación marcada como leída exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar notificación como leída");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Error al marcar notificación como leída"));
            }
        }
    }
}