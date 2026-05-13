using FindMyDevice.Application.DTOs.Auth;

namespace FindMyDevice.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> SignupAsync(SignupRequestDto request, CancellationToken cancellationToken);

        Task<AuthResult> SigninAsync(SigninRequestDto request, CancellationToken cancellationToken);

        Task<AuthResult> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken);

        Task<AuthResult> RevokeAsync(RevokeRequestDto request, CancellationToken cancellationToken);

        Task<AuthResult> SigninWithGoogleAsync(SigninWithGoogleRequestDto request, CancellationToken cancellationToken);

        Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken);

        Task<AuthResult> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken);
    }
}
