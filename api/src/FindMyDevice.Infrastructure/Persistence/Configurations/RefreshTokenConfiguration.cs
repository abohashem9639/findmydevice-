using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.ExpiresAtUtc)
                .IsRequired();

            builder.Property(x => x.DeviceInfo)
                .HasMaxLength(256);

            builder.HasIndex(x => x.TokenHash)
                .IsUnique();

            builder.HasIndex(x => new { x.UserId, x.RevokedAtUtc });

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
