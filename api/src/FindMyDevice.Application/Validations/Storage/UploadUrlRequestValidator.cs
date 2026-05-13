using FluentValidation;
using FindMyDevice.Application.DTOs.Storage;

namespace FindMyDevice.Application.Validations.Storage;

public class UploadUrlRequestValidator : AbstractValidator<UploadUrlRequest>
{
    public UploadUrlRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("FileName is required.")
            .MaximumLength(255).WithMessage("FileName cannot exceed 255 characters.")
            .Must(FindMyDevice.Application.Helpers.MediaHelper.IsAllowedExtension).WithMessage("File extension is not allowed.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("ContentType is required.")
            .MaximumLength(100).WithMessage("ContentType cannot exceed 100 characters.")
            .Must(ct => ct.StartsWith("image/")).When(x => x.Folder == StorageFolder.ProfileImages)
            .WithMessage("Only image content types are allowed for profile images.");

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("SizeBytes must be greater than zero.")
            .LessThanOrEqualTo(5 * 1024 * 1024).When(x => x.Folder == StorageFolder.ProfileImages)
            .WithMessage("Profile image size cannot exceed 5MB.")
            .LessThanOrEqualTo(10 * 1024 * 1024).When(x => x.Folder != StorageFolder.ProfileImages)
            .WithMessage("File size cannot exceed 10MB limit.");
    }
}
