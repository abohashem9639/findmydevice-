namespace FindMyDevice.Domain.Entities
{
    public class Device
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public required string Name { get; set; }

        public string? Manufacturer { get; set; }

        public string? Model { get; set; }

        public string Platform { get; set; } = "Android";

        public string? OsVersion { get; set; }

        public string? FcmToken { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? LastSeenAtUtc { get; set; }

        /// <summary>Latest battery percentage (0-100), refreshed by heartbeats and location reports.</summary>
        public int? LastBatteryLevel { get; set; }

        /// <summary>Whether the device was charging at the last heartbeat.</summary>
        public bool? LastIsCharging { get; set; }

        /// <summary>"wifi" | "cellular" | "none" | "unknown" — captured at the last heartbeat.</summary>
        public string? LastNetworkType { get; set; }

        /// <summary>
        /// Whether the phone's system-wide Location services were enabled at the last
        /// heartbeat. `null` until the phone reports for the first time. `false` means
        /// the phone is alive but the user has disabled location — anti-theft signal.
        /// </summary>
        public bool? IsLocationEnabled { get; set; }

        public User? User { get; set; }
    }
}
