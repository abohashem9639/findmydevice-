using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Application.Interfaces
{
    public interface IDeviceLocationRepository
    {
        Task AddAsync(DeviceLocation location, CancellationToken cancellationToken);

        Task<IReadOnlyList<DeviceLocation>> GetByDeviceIdAsync(
            Guid deviceId,
            int limit,
            CancellationToken cancellationToken);

        Task<DeviceLocation?> GetLatestForDeviceAsync(
            Guid deviceId,
            CancellationToken cancellationToken);
    }
}
