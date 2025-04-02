using Microsoft.EntityFrameworkCore;
using GameManagement.Domain.Entities;
using System.Reflection;

namespace GameManagement.Infrastructure.Persistence
{
    public class GameManagementDbContext : DbContext
    {
        public GameManagementDbContext(DbContextOptions<GameManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<GameRoom> GameRooms { get; set; }
        public DbSet<UserGameRoom> UserGameRooms { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}