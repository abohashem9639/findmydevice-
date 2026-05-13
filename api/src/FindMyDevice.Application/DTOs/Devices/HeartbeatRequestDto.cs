namespace FindMyDevice.Application.DTOs.Devices
{
    /// <summary>
    /// "I am alive" report sent by the phone on a fixed interval, even when
    /// location services are disabled. Carries battery and connectivity state
    /// plus a flag exposing whether the user has the location toggle on.
    /// </summary>
    public class HeartbeatRequestDto
    {
        public int? BatteryLevel { get; set; }

        public bool? IsCharging { get; set; }

        public string? NetworkType { get; set; }

        public bool IsLocationEnabled { get; set; }
    }
}
