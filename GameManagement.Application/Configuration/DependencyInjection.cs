using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GameManagement.Application.Services;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Configuration;

namespace GameManagement.Application.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configurar JwtSettings
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            services.AddSingleton(jwtSettings);

            // Registrar el servicio de autenticación
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            // Nuevo servicio de GameRoom
            services.AddScoped<IGameRoomService, GameRoomService>();
            // Servicio de notificaciones
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}