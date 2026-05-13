namespace FindMyDevice.Application.DTOs.Devices
{
    public class DeviceDto
    {
        public Guid Id { get; set; }

        public required string Name { get; set; }

        public string? Manufacturer { get; set; }

        public string? Model { get; set; }

        public required string Platform { get; set; }

        public string? OsVersion { get; set; }

        public bool HasFcmToken { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? LastSeenAtUtc { get; set; }

        public bool IsOnline { get; set; }

        public int? LastBatteryLevel { get; set; }

        public bool? LastIsCharging { get; set; }

        public string? LastNetworkType { get; set; }

        public bool? IsLocationEnabled { get; set; }
    }
}
