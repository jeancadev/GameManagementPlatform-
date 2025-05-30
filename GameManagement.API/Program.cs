using FluentValidation;
using FluentValidation.AspNetCore;
using GameManagement.Application.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using GameManagement.Application.Configuration;
using GameManagement.Infrastructure;
using GameManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.RateLimiting;
using GameManagement.API.Hubs;
using GameManagement.API.RealTime;
using GameManagement.Application.RealTime;
using GameManagement.Infrastructure.Repositories;
using GameManagement.Application.Interfaces;
using GameManagement.Application.Services;
using GameManagement.Infrastructure.Data;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Configurar Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Configurar nivel de logging según el ambiente
if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// Agregar configuración de DbContext con resiliencia para errores transitorios
builder.Services.AddDbContext<GameManagementDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(30);
        }
    ));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(30);
        }
    ));

// Configurar controladores con opciones para manejar referencias circulares
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Agregar servicios
builder.Services.AddSignalR();

// Configurar FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegistrationRequestValidator>();

// Agregar servicios de aplicación
builder.Services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();
builder.Services.AddScoped<IModerationRepository, ModerationRepository>();
builder.Services.AddScoped<IModerationService, ModerationService>();

// Registrar servicios de infraestructura
GameManagement.Infrastructure.DependencyInjection.AddInfrastructureServices(builder.Services, builder.Configuration);

// Agregar servicios de la aplicación
builder.Services.AddApplicationServices(builder.Configuration);

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Game Management Platform API",
        Version = "v1",
        Description = "API para la gestión de partidas multijugador",
        Contact = new OpenApiContact
        {
            Name = "Nombre",
            Email = "email@ejemplo.com"
        }
    });

    // Configurar la autenticación JWT en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Solucionar problemas de ciclos en los modelos
    options.CustomSchemaIds(type => type.FullName);
});

// Configurar opciones de JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);

if (jwtSettings == null)
{
    throw new InvalidOperationException("La sección JwtSettings no está configurada correctamente.");
}

var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();

            // Extraer el token de "Authorization" y eliminar el prefijo "Bearer "
            var authorization = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authorization) &&
                authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authorization.Substring("Bearer ".Length).Trim();
                context.Token = token;
                logger.LogDebug("Token recibido y procesado: {Token}", token);
            }
            else
            {
                logger.LogWarning("Token recibido con formato incorrecto: {Token}", authorization);
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();

            // Registrar el error completo para depuración
            logger.LogError("Error de autenticación detallado: {Error}\nStackTrace: {StackTrace}", 
                context.Exception.Message, 
                context.Exception.StackTrace);

            // Intentar depurar el token
            try 
            {
                var handler = new JwtSecurityTokenHandler();
                var token = context.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last();

                if (!string.IsNullOrEmpty(token))
                {
                    // Intentar leer el token sin validación para diagnosticar
                    logger.LogInformation("Intentando depurar token...");
                    if (handler.CanReadToken(token)) 
                    {
                        var jwtToken = handler.ReadJwtToken(token);
                        logger.LogInformation("Token leído correctamente. Header: {Header}, Payload: {Payload}", 
                            jwtToken.Header.SerializeToJson(), 
                            jwtToken.Payload.SerializeToJson());
                    }
                    else 
                    {
                        logger.LogError("No se puede leer el token proporcionado");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error al depurar token: {Error}", ex.Message);
            }

            logger.LogError("Error de autenticación: {Error}", context.Exception.Message);
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers["Token-Expired"] = "true";
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token validado exitosamente para el usuario: {User}",
                context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };

});


// Configurar Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Aplicar migraciones automáticamente al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Intentando aplicar migraciones de base de datos...");
        
        var gameDbContext = services.GetRequiredService<GameManagementDbContext>();
        gameDbContext.Database.Migrate();
        
        var appDbContext = services.GetRequiredService<ApplicationDbContext>();
        appDbContext.Database.Migrate();
        
        // Sembrar usuario de prueba si no existe
        var authService = services.GetRequiredService<GameManagement.Application.Interfaces.IAuthenticationService>();
        if (await SeedTestUserAsync(services, logger))
        {
            logger.LogInformation("Usuario de prueba creado exitosamente");
        }
        
        logger.LogInformation("Migraciones aplicadas exitosamente");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al aplicar migraciones de base de datos");
    }
}

// Método para sembrar usuario de prueba
async Task<bool> SeedTestUserAsync(IServiceProvider services, ILogger logger)
{
    try
    {
        var userRepository = services.GetRequiredService<GameManagement.Domain.Interfaces.IUserRepository>();
        var testUser = await userRepository.GetByUsernameAsync("testuser");
        
        if (testUser == null)
        {
            logger.LogInformation("Creando usuario de prueba: testuser");
            
            var authService = services.GetRequiredService<GameManagement.Application.Interfaces.IAuthenticationService>();
            var result = await authService.RegisterAsync(new GameManagement.Application.DTOs.RegistrationRequest
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Test123!"
            });
            
            return result.Success;
        }
        
        return true; // El usuario ya existe
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al crear usuario de prueba");
        return false;
    }
}

// Configurar el pipeline de HTTP request
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Habilita la página de errores detallados en desarrollo
}

// Configurar Swagger (disponible en todos los entornos)
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Game Management Platform API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseStaticFiles();  // Habilitar archivos estáticos

// Habilitar CORS
app.UseCors("AllowAll");

// Configurar middleware para manejo global de errores
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Error no controlado: {Message}", exception?.Message);

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            error = "Ha ocurrido un error interno. Por favor, inténtelo más tarde."
        }));
    });
});

app.UseRouting();
// Agregar autenticación y autorización al pipeline
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapHub<GameHub>("/gamehub");
app.MapControllers();

// Imprimir las URLs en la consola para referencia
var urls = app.Urls.ToList();
if (urls.Any())
{
    Console.WriteLine("La aplicación está disponible en las siguientes URLs:");
    foreach (var url in urls)
    {
        Console.WriteLine($"- {url}");
        Console.WriteLine($"- {url}/swagger");
    }
}

app.Run();