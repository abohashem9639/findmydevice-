using FluentValidation;
using FindMyDevice.Application.DTOs.Auth;

namespace FindMyDevice.Application.Validations.Auth
{
    public class RevokeRequestValidator : AbstractValidator<RevokeRequestDto>
    {
        public RevokeRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty();
        }
    }
}
