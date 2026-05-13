using FindMyDevice.Application.DTOs.DeviceLocations;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Application.Services;

public class DeviceLocationService : IDeviceLocationService
{
    private readonly IDeviceLocationRepository _locationRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeviceLocationService(
        IDeviceLocationRepository locationRepository,
        IDeviceRepository deviceRepository,
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository;
        _deviceRepository = deviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LocationDto?> ReportLocationAsync(
        Guid userId,
        Guid deviceId,
        ReportLocationRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdForUserAsync(deviceId, userId, cancellationToken);
        if (device is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        var location = new DeviceLocation
        {
            Id = Guid.CreateVersion7(),
            DeviceId = deviceId,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Accuracy = dto.Accuracy,
            Altitude = dto.Altitude,
            Speed = dto.Speed,
            Bearing = dto.Bearing,
            RecordedAtUtc = dto.RecordedAtUtc,
            ReceivedAtUtc = now,
            BatteryLevel = dto.BatteryLevel,
            IsCharging = dto.IsCharging,
            NetworkType = Normalize(dto.NetworkType),
        };

        await _locationRepository.AddAsync(location, cancellationToken);

        // Mirror the live cached fields so the device stays "alive" in dashboards
        // even between heartbeats.
        device.LastSeenAtUtc = now;
        device.LastBatteryLevel = dto.BatteryLevel ?? device.LastBatteryLevel;
        device.LastIsCharging = dto.IsCharging ?? device.LastIsCharging;
        device.LastNetworkType = Normalize(dto.NetworkType) ?? device.LastNetworkType;
        // If we received a location, location services were enabled on the phone.
        device.IsLocationEnabled = true;
        await _deviceRepository.UpdateAsync(device, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(location);
    }

    public async Task<IReadOnlyList<LocationDto>> GetLocationsAsync(
        Guid userId,
        Guid deviceId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdForUserAsync(deviceId, userId, cancellationToken);
        if (device is null)
        {
            return Array.Empty<LocationDto>();
        }

        var locations = await _locationRepository.GetByDeviceIdAsync(deviceId, limit, cancellationToken);
        return locations.Select(MapToDto).ToList();
    }

    public async Task<LocationDto?> GetLatestLocationAsync(
        Guid userId,
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdForUserAsync(deviceId, userId, cancellationToken);
        if (device is null)
        {
            return null;
        }

        var location = await _locationRepository.GetLatestForDeviceAsync(deviceId, cancellationToken);
        return location is null ? null : MapToDto(location);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static LocationDto MapToDto(DeviceLocation location) => new()
    {
        Id = location.Id,
        DeviceId = location.DeviceId,
        Latitude = location.Latitude,
        Longitude = location.Longitude,
        Accuracy = location.Accuracy,
        Altitude = location.Altitude,
        Speed = location.Speed,
        Bearing = location.Bearing,
        RecordedAtUtc = location.RecordedAtUtc,
        ReceivedAtUtc = location.ReceivedAtUtc,
        BatteryLevel = location.BatteryLevel,
        IsCharging = location.IsCharging,
        NetworkType = location.NetworkType,
    };
}
