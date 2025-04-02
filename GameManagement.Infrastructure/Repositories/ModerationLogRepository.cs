using GameManagement.Domain.Entities;
using GameManagement.Domain.Interfaces;
using GameManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameManagement.Infrastructure.Repositories
{
    public class ModerationLogRepository : IModerationLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ModerationLogRepository> _logger;

        public ModerationLogRepository(
            ApplicationDbContext context,
            ILogger<ModerationLogRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateAsync(ModeratorLogEntry entry)
    {
        try
        {
            _context.ModeratorLogs.Add(entry);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created moderation log entry: {Action} in room {RoomId}",
                entry.Action, entry.RoomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating moderation log entry");
            throw;
        }
    }

    public async Task<IEnumerable<ModeratorLogEntry>> GetRoomActivityAsync(Guid roomId)
    {
        try
        {
            return await _context.ModeratorLogs
                .Where(log => log.RoomId == roomId)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving room activity for room {RoomId}", roomId);
            throw;
        }
    }

    public async Task<IEnumerable<ModeratorLogEntry>> GetUserActivityAsync(Guid userId)
    {
        try
        {
            return await _context.ModeratorLogs
                .Where(log => log.TargetUserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity for user {UserId}", userId);
            throw;
        }
    }
}
}