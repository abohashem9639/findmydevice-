using FluentValidation;
using FindMyDevice.Application.DTOs.Auth;

namespace FindMyDevice.Application.Validations.Auth
{
    public class SigninRequestValidator : AbstractValidator<SigninRequestDto>
    {
        public SigninRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
