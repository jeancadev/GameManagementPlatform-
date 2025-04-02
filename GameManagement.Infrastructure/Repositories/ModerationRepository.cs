using GameManagement.Domain.Entities;
using GameManagement.Domain.Interfaces;
using GameManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameManagement.Application.DTOs.Moderation;

namespace GameManagement.Infrastructure.Repositories
{

    public class ModerationRepository : IModerationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ModerationRepository> _logger;

    public ModerationRepository(
        ApplicationDbContext context,
        ILogger<ModerationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ModeratorLogEntry>> GetRoomActivityAsync(Guid roomId)
    {
        try
        {
            return await _context.ModeratorLogs
                .Where(log => log.RoomId == roomId)
                .OrderByDescending(log => log.Timestamp)
                .Take(100) // Limitamos a los últimos 100 registros
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener actividad de la sala {RoomId}", roomId);
            throw;
        }
    }

    public async Task<ModeratorLogEntry> CreateLogEntryAsync(ModeratorLogEntry entry)
    {
        try
        {
            _context.ModeratorLogs.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear entrada de registro de moderación");
            throw;
        }
    }

    public async Task<IEnumerable<ModeratorLogEntry>> GetUserActivityAsync(Guid userId)
    {
        try
        {
            return await _context.ModeratorLogs
                .Where(log => log.TargetUserId == userId || log.ModeratorId == userId)
                .OrderByDescending(log => log.Timestamp)
                .Take(50)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener actividad del usuario {UserId}", userId);
            throw;
        }
    }

    public async Task ClearOldLogsAsync(DateTime olderThan)
    {
        try
        {
            var oldLogs = await _context.ModeratorLogs
                .Where(log => log.Timestamp < olderThan)
                .ToListAsync();

            _context.ModeratorLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Se eliminaron {Count} registros antiguos", oldLogs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar registros antiguos");
            throw;
        }
        }
    }
}