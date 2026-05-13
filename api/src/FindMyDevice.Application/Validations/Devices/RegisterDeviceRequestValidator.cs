using FluentValidation;
using FindMyDevice.Application.DTOs.Devices;

namespace FindMyDevice.Application.Validations.Devices
{
    public class RegisterDeviceRequestValidator : AbstractValidator<RegisterDeviceRequestDto>
    {
        private static readonly string[] AllowedPlatforms = ["Android", "iOS"];

        public RegisterDeviceRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Manufacturer)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Manufacturer));

            RuleFor(x => x.Model)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Model));

            RuleFor(x => x.Platform)
                .Must(p => AllowedPlatforms.Contains(p))
                .WithMessage($"Platform must be one of: {string.Join(", ", AllowedPlatforms)}.")
                .When(x => !string.IsNullOrWhiteSpace(x.Platform));

            RuleFor(x => x.OsVersion)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.OsVersion));

            RuleFor(x => x.FcmToken)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.FcmToken));
        }
    }
}
