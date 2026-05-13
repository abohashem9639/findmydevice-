using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Application.Interfaces
{
    public interface IDeviceRepository
    {
        Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<Device?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken);

        Task<IReadOnlyList<Device>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task AddAsync(Device device, CancellationToken cancellationToken);

        Task UpdateAsync(Device device, CancellationToken cancellationToken);

        Task DeleteAsync(Device device, CancellationToken cancellationToken);
    }
}
