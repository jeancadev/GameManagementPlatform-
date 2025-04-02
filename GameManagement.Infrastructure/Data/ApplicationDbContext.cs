
using GameManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameManagement.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<ModeratorLogEntry> ModeratorLogs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ModeratorLogEntry>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Details)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Timestamp)
                    .IsRequired();

                entity.HasOne<GameRoom>()
                    .WithMany()
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ModeratorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.TargetUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}