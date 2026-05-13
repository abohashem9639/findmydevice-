using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Infrastructure.Persistence.Configurations
{
    public class DeviceLocationConfiguration : IEntityTypeConfiguration<DeviceLocation>
    {
        public void Configure(EntityTypeBuilder<DeviceLocation> builder)
        {
            builder.ToTable("device_locations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Latitude).IsRequired();
            builder.Property(x => x.Longitude).IsRequired();
            builder.Property(x => x.Accuracy).IsRequired();
            builder.Property(x => x.Altitude);
            builder.Property(x => x.Speed);
            builder.Property(x => x.Bearing);

            builder.Property(x => x.RecordedAtUtc).IsRequired();
            builder.Property(x => x.ReceivedAtUtc).IsRequired();

            builder.Property(x => x.BatteryLevel);
            builder.Property(x => x.IsCharging);
            builder.Property(x => x.NetworkType).HasMaxLength(20);

            // Compound index optimised for "give me the latest N locations for this device".
            builder.HasIndex(x => new { x.DeviceId, x.RecordedAtUtc });

            builder.HasOne(x => x.Device)
                .WithMany()
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
