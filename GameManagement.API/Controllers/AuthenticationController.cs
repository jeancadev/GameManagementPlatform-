using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace GameManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(
            IAuthenticationService authenticationService,
            ILogger<AuthenticationController> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Registrar un nuevo usuario",
            Description = "Crea una nueva cuenta de usuario en el sistema",
            OperationId = "Register",
            Tags = new[] { "Autenticación" }
        )]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            _logger.LogInformation("Iniciando proceso de registro para usuario: {Username}", request.Username);

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Datos de registro inválidos", errors));
                }

                var result = await _authenticationService.RegisterAsync(request);

                if (!result.Success)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
                }

                return Ok(ApiResponse<AuthenticationResponse>.SuccessResponse(
                    result, "Registro exitoso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro del usuario {Username}", request.Username);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Ha ocurrido un error interno al procesar su solicitud"));
            }
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Iniciar sesión",
            Description = "Autentica un usuario existente y devuelve un token JWT",
            OperationId = "Login",
            Tags = new[] { "Autenticación" }
        )]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
        {
            _logger.LogInformation("Iniciando proceso de login para usuario: {Username}", request.Username);

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Datos de login inválidos", errors));
                }

                var result = await _authenticationService.AuthenticateAsync(request);

                if (!result.Success)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse(result.Message));
                }

                return Ok(ApiResponse<AuthenticationResponse>.SuccessResponse(
                    result, "Autenticación exitosa"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la autenticación del usuario {Username}", request.Username);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Ha ocurrido un error interno al procesar su solicitud"));
            }
        }
    }
}