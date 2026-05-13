namespace FindMyDevice.Domain.Entities
{
    public class DeviceLocation
    {
        public Guid Id { get; set; }

        public Guid DeviceId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public float Accuracy { get; set; }

        public double? Altitude { get; set; }

        public float? Speed { get; set; }

        public float? Bearing { get; set; }

        /// <summary>When the phone captured the GPS fix.</summary>
        public DateTime RecordedAtUtc { get; set; }

        /// <summary>When the server received the report (set server-side).</summary>
        public DateTime ReceivedAtUtc { get; set; }

        public int? BatteryLevel { get; set; }

        public bool? IsCharging { get; set; }

        public string? NetworkType { get; set; }

        public Device? Device { get; set; }
    }
}
