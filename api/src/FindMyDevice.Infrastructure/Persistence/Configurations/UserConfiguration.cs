using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(254);

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.Property(x => x.PasswordHash)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(x => x.DisplayName)
                .HasMaxLength(100);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.LastLoginAtUtc);

            builder.Property(x => x.FailedAccessCount)
                .HasDefaultValue(0);

            builder.Property(x => x.LockoutEndUtc);

            builder.Property(x => x.GoogleId)
                .HasColumnName("google_id")
                .HasMaxLength(100);

            builder.Property(x => x.Language)
                .HasColumnName("language")
                .HasMaxLength(10);

            builder.Property(x => x.PasswordResetToken)
                .HasColumnName("password_reset_token")
                .HasMaxLength(100);

            builder.Property(x => x.PasswordResetExpiresAtUtc)
                .HasColumnName("password_reset_expires_at_utc");

            builder.Property(x => x.ProfileImage)
                .HasMaxLength(2048);
        }
    }
}
