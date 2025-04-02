using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace GameManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("public")]
        public IActionResult PublicEndpoint()
        {
            return Ok(new { message = "Este es un endpoint público" });
        }

        [Authorize]
        [HttpGet("protected")]
        public IActionResult ProtectedEndpoint()
        {
            try
            {
                _logger.LogInformation("Iniciando endpoint protegido");

                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                _logger.LogInformation("Authorization Header: {Header}", authHeader);

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                _logger.LogInformation("Claims encontrados - UserId: {UserId}, Username: {Username}",
                    userId ?? "no encontrado",
                    username ?? "no encontrado");

                return Ok(new
                {
                    message = "Este es un endpoint protegido",
                    userId = userId,
                    username = username,
                    claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en endpoint protegido");
                throw;
            }
        }
    }
}