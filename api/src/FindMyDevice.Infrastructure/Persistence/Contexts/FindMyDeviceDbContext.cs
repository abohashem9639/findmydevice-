using Microsoft.EntityFrameworkCore;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Infrastructure.Persistence.Contexts
{
    public class FindMyDeviceDbContext : DbContext
    {
        public FindMyDeviceDbContext(DbContextOptions<FindMyDeviceDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<Device> Devices => Set<Device>();

        public DbSet<DeviceLocation> DeviceLocations => Set<DeviceLocation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FindMyDeviceDbContext).Assembly);
        }
    }
}
