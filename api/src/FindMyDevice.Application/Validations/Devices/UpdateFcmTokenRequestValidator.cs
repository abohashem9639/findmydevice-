using FluentValidation;
using FindMyDevice.Application.DTOs.Devices;

namespace FindMyDevice.Application.Validations.Devices
{
    public class UpdateFcmTokenRequestValidator : AbstractValidator<UpdateFcmTokenRequestDto>
    {
        public UpdateFcmTokenRequestValidator()
        {
            RuleFor(x => x.FcmToken)
                .NotEmpty()
                .MaximumLength(500);
        }
    }
}
