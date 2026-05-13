using Microsoft.EntityFrameworkCore;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;
using FindMyDevice.Infrastructure.Persistence.Contexts;

namespace FindMyDevice.Infrastructure.Persistence.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly FindMyDeviceDbContext _dbContext;

        public DeviceRepository(FindMyDeviceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            _dbContext.Devices.SingleOrDefaultAsync(d => d.Id == id, cancellationToken);

        public Task<Device?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken) =>
            _dbContext.Devices.SingleOrDefaultAsync(d => d.Id == id && d.UserId == userId, cancellationToken);

        public async Task<IReadOnlyList<Device>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Devices
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Device device, CancellationToken cancellationToken)
        {
            await _dbContext.Devices.AddAsync(device, cancellationToken);
        }

        public Task UpdateAsync(Device device, CancellationToken cancellationToken)
        {
            _dbContext.Devices.Update(device);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Device device, CancellationToken cancellationToken)
        {
            _dbContext.Devices.Remove(device);
            return Task.CompletedTask;
        }
    }
}
