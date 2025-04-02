using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GameManagement.Domain.Entities;

namespace GameManagement.Infrastructure.Persistence.Configurations
{
    public class GameRoomConfiguration : IEntityTypeConfiguration<GameRoom>
    {
        public void Configure(EntityTypeBuilder<GameRoom> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.MaxPlayers)
                .IsRequired();

            builder.Property(e => e.MinPlayersToStart)
                .IsRequired();

            builder.Property(e => e.Status)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.MaxWaitTimeToStart)
                .IsRequired();

            builder.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.UserRooms)
                .WithOne(ur => ur.GameRoom)
                .HasForeignKey(ur => ur.GameRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(e => e.UserRooms)
                .AutoInclude();
        }
    }
}