using FindMyDevice.Application.DTOs.Devices;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Application.Services;

public class DeviceService : IDeviceService
{
    private static readonly TimeSpan OnlineThreshold = TimeSpan.FromMinutes(2);

    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeviceService(IDeviceRepository deviceRepository, IUnitOfWork unitOfWork)
    {
        _deviceRepository = deviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeviceDto> RegisterDeviceAsync(Guid userId, RegisterDeviceRequestDto dto, CancellationToken cancellationToken = default)
    {
        var device = new Device
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Name = dto.Name.Trim(),
            Manufacturer = Normalize(dto.Manufacturer),
            Model = Normalize(dto.Model),
            Platform = string.IsNullOrWhiteSpace(dto.Platform) ? "Android" : dto.Platform.Trim(),
            OsVersion = Normalize(dto.OsVersion),
            FcmToken = Normalize(dto.FcmToken),
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _deviceRepository.AddAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(device);
    }

    public async Task<IReadOnlyList<DeviceDto>> GetUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var devices = await _deviceRepository.GetByUserIdAsync(userId, cancellationToken);
        return devices.Select(MapToDto).ToList();
    }

    public async Task<DeviceDto?> GetDeviceAsync(Guid userId, Guid deviceId, CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdForUserAsync(deviceId, userId, cancellationToken);
        return device is null ? null : MapToDto(device);
    }

    public async Task<DeviceDto?> UpdateFcmTokenAsync(Guid userId, Guid deviceId, string fcmToken, CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdForUserAsync(deviceId, userId, cancellationToken);
        if (device is null)
        {
            return null;
        }

        device.FcmToken = fcmToken.Trim();
        await _deviceRepository.UpdateAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(device);
    }

    public async Task<bool> DeleteDeviceAsync(Guid userId, Guid deviceId, CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdForUserAsync(deviceId, userId, cancellationToken);
        if (device is null)
        {
            return false;
        }

        await _deviceRepository.DeleteAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DeviceDto?> RecordHeartbeatAsync(
        Guid userId,
        Guid deviceId,
        HeartbeatRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdForUserAsync(deviceId, userId, cancellationToken);
        if (device is null)
        {
            return null;
        }

        device.LastSeenAtUtc = DateTime.UtcNow;
        device.LastBatteryLevel = dto.BatteryLevel;
        device.LastIsCharging = dto.IsCharging;
        device.LastNetworkType = Normalize(dto.NetworkType);
        device.IsLocationEnabled = dto.IsLocationEnabled;

        await _deviceRepository.UpdateAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(device);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DeviceDto MapToDto(Device device) => new()
    {
        Id = device.Id,
        Name = device.Name,
        Manufacturer = device.Manufacturer,
        Model = device.Model,
        Platform = device.Platform,
        OsVersion = device.OsVersion,
        HasFcmToken = !string.IsNullOrEmpty(device.FcmToken),
        CreatedAtUtc = device.CreatedAtUtc,
        LastSeenAtUtc = device.LastSeenAtUtc,
        IsOnline = device.LastSeenAtUtc.HasValue && (DateTime.UtcNow - device.LastSeenAtUtc.Value) < OnlineThreshold,
        LastBatteryLevel = device.LastBatteryLevel,
        LastIsCharging = device.LastIsCharging,
        LastNetworkType = device.LastNetworkType,
        IsLocationEnabled = device.IsLocationEnabled,
    };
}
