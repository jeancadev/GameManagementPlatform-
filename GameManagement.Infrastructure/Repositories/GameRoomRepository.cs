using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using GameManagement.Domain.Entities;
using GameManagement.Domain.Interfaces;
using GameManagement.Domain.Enums;
using GameManagement.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace GameManagement.Infrastructure.Repositories
{
    public class GameRoomRepository : IGameRoomRepository
    {
        private readonly GameManagementDbContext _context;
        private readonly ILogger<GameRoomRepository> _logger;

        public GameRoomRepository(
            GameManagementDbContext context,
            ILogger<GameRoomRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GameRoom> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.GameRooms
                    .Include(r => r.Owner)
                    .Include(r => r.UserRooms)
                        .ThenInclude(ur => ur.User)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sala por ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<GameRoom>> GetAvailableRoomsAsync()
        {
            try
            {
                return await _context.GameRooms
                    .Include(r => r.Owner)
                    .Include(r => r.UserRooms)
                        .ThenInclude(ur => ur.User)
                    .Where(r => r.Status == GameRoomStatus.Created)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener salas disponibles");
                throw;
            }
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            try
            {
                return await _context.GameRooms
                    .AnyAsync(r => r.Name.ToLower() == name.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de sala por nombre: {Name}", name);
                throw;
            }
        }

        public async Task<IEnumerable<GameRoom>> GetActiveRoomsByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Buscando salas activas para usuario: {UserId}", userId);

            try
            {
                var activeRooms = await _context.GameRooms
                    .Include(r => r.Owner)
                    .Include(r => r.UserRooms)
                        .ThenInclude(ur => ur.User)
                    .Where(r => r.Status == GameRoomStatus.Created &&
                               (r.OwnerId == userId || r.UserRooms.Any(ur => ur.UserId == userId)))
                    .ToListAsync();

                _logger.LogInformation("Salas activas encontradas: {Count}", activeRooms.Count);
                return activeRooms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener salas activas para el usuario: {UserId}", userId);
                throw;
            }
        }

        public async Task CreateAsync(GameRoom gameRoom)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de sala de juego con ID: {GameRoomId}", gameRoom.Id);
                await _context.GameRooms.AddAsync(gameRoom);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Sala de juego creada exitosamente");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al crear sala de juego. Detalles: {Message}",
                    ex.InnerException?.Message);
                throw new InvalidOperationException(
                    $"Error al guardar la sala de juego en la base de datos: {ex.InnerException?.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear sala de juego");
                throw new InvalidOperationException(
                    $"Error inesperado al crear la sala de juego: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(GameRoom gameRoom)
        {
            try
            {
                _logger.LogInformation("Actualizando sala de juego: {GameRoomId}", gameRoom.Id);
                _context.GameRooms.Update(gameRoom);

                // Obtener los cambios que se intentan guardar
                var changes = _context.ChangeTracker.Entries()
                    .Where(e => e.State != EntityState.Unchanged)
                    .ToList();

                foreach (var change in changes)
                {
                    _logger.LogInformation("Cambio detectado: Entidad {Entity}, Estado {State}",
                        change.Entity.GetType().Name,
                        change.State);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Sala de juego actualizada exitosamente");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar sala de juego: {GameRoomId}", gameRoom.Id);
                throw new InvalidOperationException($"Error al actualizar la sala de juego: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var gameRoom = await GetByIdAsync(id);
                if (gameRoom != null)
                {
                    _logger.LogInformation("Eliminando sala de juego: {GameRoomId}", id);
                    _context.GameRooms.Remove(gameRoom);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Sala de juego eliminada exitosamente");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar sala de juego: {GameRoomId}", id);
                throw new InvalidOperationException($"Error al eliminar la sala de juego: {ex.Message}", ex);
            }
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                return await _context.GameRooms.AnyAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de sala: {GameRoomId}", id);
                throw;
            }
        }
    }
}