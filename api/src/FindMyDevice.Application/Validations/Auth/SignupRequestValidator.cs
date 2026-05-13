using FluentValidation;
using FindMyDevice.Application.DTOs.Auth;

namespace FindMyDevice.Application.Validations.Auth
{
    public class SignupRequestValidator : AbstractValidator<SignupRequestDto>
    {
        public SignupRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match.");

            RuleFor(x => x.DisplayName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
        }
    }
}
