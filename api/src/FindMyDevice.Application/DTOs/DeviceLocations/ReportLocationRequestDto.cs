namespace FindMyDevice.Application.DTOs.DeviceLocations
{
    public class ReportLocationRequestDto
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public float Accuracy { get; set; }

        public double? Altitude { get; set; }

        public float? Speed { get; set; }

        public float? Bearing { get; set; }

        /// <summary>When the phone captured the GPS fix (UTC, ISO 8601).</summary>
        public DateTime RecordedAtUtc { get; set; }

        public int? BatteryLevel { get; set; }

        public bool? IsCharging { get; set; }

        public string? NetworkType { get; set; }
    }
}
