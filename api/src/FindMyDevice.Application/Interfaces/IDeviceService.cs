using FindMyDevice.Application.DTOs.Devices;

namespace FindMyDevice.Application.Interfaces
{
    public interface IDeviceService
    {
        Task<DeviceDto> RegisterDeviceAsync(Guid userId, RegisterDeviceRequestDto dto, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<DeviceDto>> GetUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<DeviceDto?> GetDeviceAsync(Guid userId, Guid deviceId, CancellationToken cancellationToken = default);

        Task<DeviceDto?> UpdateFcmTokenAsync(Guid userId, Guid deviceId, string fcmToken, CancellationToken cancellationToken = default);

        Task<bool> DeleteDeviceAsync(Guid userId, Guid deviceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Records a lightweight "I am alive" report. Updates LastSeenAtUtc and
        /// the cached battery/network/location-enabled flags without persisting
        /// any new row in device_locations.
        /// Returns the updated DeviceDto, or null if the device isn't owned by the user.
        /// </summary>
        Task<DeviceDto?> RecordHeartbeatAsync(
            Guid userId,
            Guid deviceId,
            HeartbeatRequestDto dto,
            CancellationToken cancellationToken = default);
    }
}
