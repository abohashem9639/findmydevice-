using FluentValidation;
using FindMyDevice.Application.DTOs.Users;

namespace FindMyDevice.Application.Validations.Users;

public class UpdateProfileImageDtoValidator : AbstractValidator<UpdateProfileImageDto>
{
    public UpdateProfileImageDtoValidator()
    {
        RuleFor(x => x.FileKey)
            .NotEmpty().WithMessage("FileKey is required.")
            .MaximumLength(2048).WithMessage("FileKey cannot exceed 2048 characters.");
    }
}
