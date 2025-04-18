using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Collections.Generic;
using GameManagement.Application.DTOs;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Interfaces;
using GameManagement.Application.Configuration;
using GameManagement.Application.Interfaces;

namespace GameManagement.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IUserRepository userRepository,
            JwtSettings jwtSettings,
            ILogger<AuthenticationService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return AuthenticationResponse.Failed("Invalid credentials");
            }

            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                return AuthenticationResponse.Failed("Invalid username or password");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return AuthenticationResponse.Failed("Invalid username or password");
            }

            user.UpdateLastLogin();
            await _userRepository.UpdateAsync(user);

            var token = GenerateJwtToken(user);
            return AuthenticationResponse.Successful(token, user.Username);
        }

        public async Task<AuthenticationResponse> RegisterAsync(RegistrationRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando proceso de registro para usuario: {Username}", request.Username);

                if (await _userRepository.ExistsAsync(request.Email, request.Username))
                {
                    _logger.LogWarning("Usuario o email ya existe: {Username}, {Email}", request.Username, request.Email);
                    return AuthenticationResponse.Failed("Username or email already exists");
                }

                _logger.LogInformation("Creando hash de contraseña para usuario: {Username}", request.Username);
                CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

                _logger.LogInformation("Creando entidad de usuario para: {Username}", request.Username);
                var user = User.Create(request.Username, request.Email);
                user.SetPassword(passwordHash, passwordSalt);

                _logger.LogInformation("Guardando usuario en base de datos: {Username}", request.Username);
                await _userRepository.CreateAsync(user);

                _logger.LogInformation("Generando token JWT para usuario: {Username}", request.Username);
                var token = GenerateJwtToken(user);

                _logger.LogInformation("Registro completado exitosamente para: {Username}", request.Username);
                return AuthenticationResponse.Successful(token, user.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro del usuario {Username}: {Error}",
                    request.Username, ex.ToString());
                throw;
            }
        }

        private string GenerateJwtToken(User user)
        {
            _logger.LogDebug("Generando token JWT para el usuario: {Username}", user.Username);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                NotBefore = DateTime.UtcNow,
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            _logger.LogDebug("Token JWT generado: {Token}", tokenString);
            
            return tokenString;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }
    }
}