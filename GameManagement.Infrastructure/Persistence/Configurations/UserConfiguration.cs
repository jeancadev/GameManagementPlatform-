using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GameManagement.Domain.Entities;

namespace GameManagement.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Username)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Email)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(e => e.Username)
                .IsUnique();

            builder.HasIndex(e => e.Email)
                .IsUnique();

            builder.Property(e => e.PasswordHash)
                .IsRequired();

            builder.Property(e => e.PasswordSalt)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .IsRequired();
        }
    }
}