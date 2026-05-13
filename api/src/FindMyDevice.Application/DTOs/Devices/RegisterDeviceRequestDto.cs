namespace FindMyDevice.Application.DTOs.Devices
{
    public class RegisterDeviceRequestDto
    {
        public required string Name { get; set; }

        public string? Manufacturer { get; set; }

        public string? Model { get; set; }

        public string? Platform { get; set; }

        public string? OsVersion { get; set; }

        public string? FcmToken { get; set; }
    }
}
