using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GameManagement.Domain.Entities;

namespace GameManagement.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(n => n.Type)
                .IsRequired();

            builder.Property(n => n.CreatedAt)
                .IsRequired();

            builder.HasOne(n => n.Room)
                .WithMany()
                .HasForeignKey(n => n.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(n => n.Sender)
                .WithMany()
                .HasForeignKey(n => n.SenderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(n => n.Receiver)
                .WithMany()
                .HasForeignKey(n => n.ReceiverId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}