using FluentValidation;
using FindMyDevice.Application.DTOs.DeviceLocations;

namespace FindMyDevice.Application.Validations.DeviceLocations
{
    public class ReportLocationRequestValidator : AbstractValidator<ReportLocationRequestDto>
    {
        private static readonly string[] AllowedNetworkTypes = ["wifi", "cellular", "none", "unknown"];

        public ReportLocationRequestValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90.0, 90.0)
                .WithMessage("Latitude must be between -90 and 90.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180.0, 180.0)
                .WithMessage("Longitude must be between -180 and 180.");

            RuleFor(x => x.Accuracy)
                .GreaterThan(0)
                .LessThan(100_000)
                .WithMessage("Accuracy must be a positive number in metres (and reasonably small).");

            RuleFor(x => x.RecordedAtUtc)
                .NotEmpty()
                .Must(NotBeInTheFuture)
                .WithMessage("RecordedAtUtc cannot be in the future.")
                .Must(NotBeTooOld)
                .WithMessage("RecordedAtUtc cannot be older than 24 hours.");

            RuleFor(x => x.BatteryLevel)
                .InclusiveBetween(0, 100)
                .When(x => x.BatteryLevel.HasValue)
                .WithMessage("BatteryLevel must be between 0 and 100.");

            RuleFor(x => x.NetworkType)
                .Must(v => AllowedNetworkTypes.Contains(v))
                .When(x => !string.IsNullOrWhiteSpace(x.NetworkType))
                .WithMessage($"NetworkType must be one of: {string.Join(", ", AllowedNetworkTypes)}.");

            RuleFor(x => x.Bearing)
                .InclusiveBetween(0f, 360f)
                .When(x => x.Bearing.HasValue);
        }

        // Tolerate up to 5 minutes of clock skew between phone and server.
        private static bool NotBeInTheFuture(DateTime timestamp) =>
            timestamp <= DateTime.UtcNow.AddMinutes(5);

        private static bool NotBeTooOld(DateTime timestamp) =>
            timestamp >= DateTime.UtcNow.AddDays(-1);
    }
}
