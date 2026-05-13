namespace FindMyDevice.Application.DTOs.DeviceLocations
{
    public class LocationDto
    {
        public Guid Id { get; set; }

        public Guid DeviceId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public float Accuracy { get; set; }

        public double? Altitude { get; set; }

        public float? Speed { get; set; }

        public float? Bearing { get; set; }

        public DateTime RecordedAtUtc { get; set; }

        public DateTime ReceivedAtUtc { get; set; }

        public int? BatteryLevel { get; set; }

        public bool? IsCharging { get; set; }

        public string? NetworkType { get; set; }
    }
}
