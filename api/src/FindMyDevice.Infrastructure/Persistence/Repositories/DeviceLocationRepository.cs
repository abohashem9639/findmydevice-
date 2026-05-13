using Microsoft.EntityFrameworkCore;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;
using FindMyDevice.Infrastructure.Persistence.Contexts;

namespace FindMyDevice.Infrastructure.Persistence.Repositories
{
    public class DeviceLocationRepository : IDeviceLocationRepository
    {
        private readonly FindMyDeviceDbContext _dbContext;

        public DeviceLocationRepository(FindMyDeviceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(DeviceLocation location, CancellationToken cancellationToken)
        {
            await _dbContext.DeviceLocations.AddAsync(location, cancellationToken);
        }

        public async Task<IReadOnlyList<DeviceLocation>> GetByDeviceIdAsync(
            Guid deviceId,
            int limit,
            CancellationToken cancellationToken)
        {
            return await _dbContext.DeviceLocations
                .AsNoTracking()
                .Where(x => x.DeviceId == deviceId)
                .OrderByDescending(x => x.RecordedAtUtc)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public Task<DeviceLocation?> GetLatestForDeviceAsync(
            Guid deviceId,
            CancellationToken cancellationToken)
        {
            return _dbContext.DeviceLocations
                .AsNoTracking()
                .Where(x => x.DeviceId == deviceId)
                .OrderByDescending(x => x.RecordedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
