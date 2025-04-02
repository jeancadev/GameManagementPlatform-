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

// Agregar configuración de DbContext
builder.Services.AddDbContext<GameManagementDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
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

    // Para documentación XML, descomentar y configurar
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // options.IncludeXmlComments(xmlPath);
});

// Configuración de autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("SecretKey no está configurado en JwtSettings.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        // Agregar logging para debugging
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token recibido: {Token}", context.Token);
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("Error de autenticación: {Error}", context.Exception.Message);
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

// Configurar el pipeline de HTTP request
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Habilita la página de errores detallados en desarrollo

    // Configurar Swagger
    app.UseSwagger(c => {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Game Management Platform API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

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

// Agregar autenticación y autorización al pipeline
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapHub<GameHub>("/gamehub");
app.MapControllers();

app.Run();