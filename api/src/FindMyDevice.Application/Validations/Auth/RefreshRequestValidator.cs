using FluentValidation;
using FindMyDevice.Application.DTOs.Auth;

namespace FindMyDevice.Application.Validations.Auth
{
    public class RefreshRequestValidator : AbstractValidator<RefreshRequestDto>
    {
        public RefreshRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty();
        }
    }
}
