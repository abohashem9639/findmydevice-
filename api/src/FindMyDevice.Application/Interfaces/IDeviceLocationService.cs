using FindMyDevice.Application.DTOs.DeviceLocations;

namespace FindMyDevice.Application.Interfaces
{
    public interface IDeviceLocationService
    {
        Task<LocationDto?> ReportLocationAsync(
            Guid userId,
            Guid deviceId,
            ReportLocationRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<LocationDto>> GetLocationsAsync(
            Guid userId,
            Guid deviceId,
            int limit,
            CancellationToken cancellationToken = default);

        Task<LocationDto?> GetLatestLocationAsync(
            Guid userId,
            Guid deviceId,
            CancellationToken cancellationToken = default);
    }
}
