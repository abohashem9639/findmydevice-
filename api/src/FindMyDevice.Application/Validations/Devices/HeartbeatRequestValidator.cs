using FluentValidation;
using FindMyDevice.Application.DTOs.Devices;

namespace FindMyDevice.Application.Validations.Devices
{
    public class HeartbeatRequestValidator : AbstractValidator<HeartbeatRequestDto>
    {
        private static readonly string[] AllowedNetworkTypes = ["wifi", "cellular", "none", "unknown"];

        public HeartbeatRequestValidator()
        {
            RuleFor(x => x.BatteryLevel)
                .InclusiveBetween(0, 100)
                .When(x => x.BatteryLevel.HasValue);

            RuleFor(x => x.NetworkType)
                .Must(v => AllowedNetworkTypes.Contains(v))
                .When(x => !string.IsNullOrWhiteSpace(x.NetworkType))
                .WithMessage($"NetworkType must be one of: {string.Join(", ", AllowedNetworkTypes)}.");
        }
    }
}
