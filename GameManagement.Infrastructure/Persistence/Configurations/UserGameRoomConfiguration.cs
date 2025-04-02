using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GameManagement.Domain.Entities;

namespace GameManagement.Infrastructure.Persistence.Configurations
{
    public class UserGameRoomConfiguration : IEntityTypeConfiguration<UserGameRoom>
    {
        public void Configure(EntityTypeBuilder<UserGameRoom> builder)
        {
            builder.HasKey(ugr => new { ugr.UserId, ugr.GameRoomId });

            builder.Property(ugr => ugr.Role)
                .IsRequired();

            builder.Property(ugr => ugr.JoinedAt)
                .IsRequired();

            builder.HasOne(ugr => ugr.User)
                .WithMany()
                .HasForeignKey(ugr => ugr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ugr => ugr.GameRoom)
                .WithMany()
                .HasForeignKey(ugr => ugr.GameRoomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}