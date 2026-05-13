using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Infrastructure.Persistence.Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.ToTable("devices");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Manufacturer)
                .HasMaxLength(100);

            builder.Property(x => x.Model)
                .HasMaxLength(100);

            builder.Property(x => x.Platform)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Android");

            builder.Property(x => x.OsVersion)
                .HasMaxLength(50);

            builder.Property(x => x.FcmToken)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.LastSeenAtUtc);

            builder.Property(x => x.LastBatteryLevel);
            builder.Property(x => x.LastIsCharging);
            builder.Property(x => x.LastNetworkType).HasMaxLength(20);
            builder.Property(x => x.IsLocationEnabled);

            builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc });

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
