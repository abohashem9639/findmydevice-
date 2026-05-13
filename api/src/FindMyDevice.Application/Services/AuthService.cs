using System.Security.Cryptography;
using System.Text;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FindMyDevice.Application.Configurations;
using FindMyDevice.Application.DTOs.Auth;
using FindMyDevice.Application.Extensions;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;
using Mapster;

namespace FindMyDevice.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly JwtOptions _jwtOptions;
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasherService passwordHasher,
            IRefreshTokenService refreshTokenService,
            IJwtTokenService jwtTokenService,
            IGoogleAuthService googleAuthService,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            IOptions<JwtOptions> jwtOptions,
            IOptions<EmailOptions> emailOptions,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _refreshTokenService = refreshTokenService;
            _jwtTokenService = jwtTokenService;
            _googleAuthService = googleAuthService;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _jwtOptions = jwtOptions.Value;
            _emailOptions = emailOptions.Value;
            _logger = logger;
        }

        public async Task<AuthResult> SignupAsync(SignupRequestDto request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.NormalizeEmail();
            if (await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
            {
                _logger.LogWarning("Signup failed: Email {Email} already registered.", normalizedEmail);
                return AuthResult.Fail(AuthError.DuplicateEmail, "Email is already registered.");
            }

            var now = DateTime.UtcNow;
            var user = await CreateUserAsync(normalizedEmail, request.DisplayName, request.Password, null, request.Language, now, cancellationToken);
            _logger.LogInformation("User created: {UserId}", user.Id);

            var result = await GenerateAuthResponseAsync(user, now, null, cancellationToken);
            if (result.Succeeded)
            {
                await SendWelcomeEmailAsync(user, cancellationToken);
            }

            return result;
        }

        public async Task<AuthResult> SigninAsync(SigninRequestDto request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.NormalizeEmail();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            var now = DateTime.UtcNow;

            if (user is null)
            {
                _logger.LogWarning("Signin failed: User not found for email {Email}.", normalizedEmail);
                return AuthResult.Fail(AuthError.InvalidCredentials, "Invalid email or password.");
            }

            if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > now)
            {
                _logger.LogWarning("Signin failed: User {UserId} is locked out.", user.Id);
                return AuthResult.Fail(AuthError.UserLockedOut, "User is locked out.");
            }

            var passwordValid = _passwordHasher.VerifyHashedPassword(user, request.Password);
            if (!passwordValid)
            {
                user.FailedAccessCount += 1;
                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("Signin failed: Invalid password for email {Email}.", normalizedEmail);
                return AuthResult.Fail(AuthError.InvalidCredentials, "Invalid email or password.");
            }

            await UpdateUserLoginStatsAsync(user, now, null, cancellationToken);
            _logger.LogInformation("User signed in: {UserId}", user.Id);

            return await GenerateAuthResponseAsync(user, now, null, cancellationToken);
        }

        public async Task<AuthResult> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var hashedToken = _refreshTokenService.HashToken(request.RefreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(hashedToken, includeUser: true, cancellationToken);

            if (storedToken is null || storedToken.RevokedAtUtc.HasValue || storedToken.ExpiresAtUtc <= now || storedToken.ReplacedByTokenId.HasValue)
            {
                _logger.LogWarning("Refresh failed: Invalid or expired token.");
                return AuthResult.Fail(AuthError.InvalidRefreshToken, "Refresh token is not valid.");
            }

            var user = storedToken.User ?? await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Refresh failed: User not found for token.");
                return AuthResult.Fail(AuthError.InvalidRefreshToken, "Refresh token is not valid.");
            }

            return await GenerateAuthResponseAsync(user, now, storedToken, cancellationToken);
        }

        public async Task<AuthResult> RevokeAsync(RevokeRequestDto request, CancellationToken cancellationToken)
        {
            var hashedToken = _refreshTokenService.HashToken(request.RefreshToken);
            var existing = await _refreshTokenRepository.GetByTokenHashAsync(hashedToken, includeUser: false, cancellationToken);

            if (existing is null || existing.RevokedAtUtc.HasValue)
            {
                _logger.LogWarning("Revoke failed: Invalid or already revoked token.");
                return AuthResult.Fail(AuthError.InvalidRefreshToken, "Refresh token is not valid or already revoked.");
            }

            existing.RevokedAtUtc = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Token revoked for user ID {UserId}.", existing.UserId);

            return AuthResult.Success(new AuthResponseDto
            {
                AccessToken = string.Empty,
                RefreshToken = string.Empty,
                ExpiresInSeconds = 0,
                TokenType = "Bearer",
            });
        }

        public async Task<AuthResult> SigninWithGoogleAsync(SigninWithGoogleRequestDto request, CancellationToken cancellationToken)
        {
            GoogleTokenPayloadDto payload;
            try
            {
                payload = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google token verification failed.");
                return AuthResult.Fail(AuthError.InvalidCredentials, "Invalid Google ID Token.");
            }

            if (payload is null)
            {
                _logger.LogWarning("Google token verification returned null payload.");
                return AuthResult.Fail(AuthError.InvalidCredentials, "Invalid Google ID Token.");
            }

            var normalizedEmail = payload.Email.NormalizeEmail();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            var now = DateTime.UtcNow;
            var isNewUser = false;

            if (user is null)
            {
                // Note: We don't have language from Google payload easily, so passing null.
                // User can update it later or we can try to extract 'locale' if added to DTO.
                user = await CreateUserAsync(normalizedEmail, payload.Name, null, payload.Subject, null, now, cancellationToken);
                isNewUser = true;
                _logger.LogInformation("New user created via Google login: {UserId}", user.Id);
            }
            else
            {
                await UpdateUserLoginStatsAsync(user, now, payload.Subject, cancellationToken);
                _logger.LogInformation("Existing user signed in via Google: {UserId}", user.Id);
            }

            if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > now)
            {
                _logger.LogWarning("Google Signin failed: User {UserId} is locked out.", user.Id);
                return AuthResult.Fail(AuthError.UserLockedOut, "User is locked out.");
            }

            var result = await GenerateAuthResponseAsync(user, now, null, cancellationToken);
            if (result.Succeeded && isNewUser)
            {
                await SendWelcomeEmailAsync(user, cancellationToken);
            }

            return result;
        }

        public async Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.NormalizeEmail();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

            // Always return success to prevent email enumeration attacks
            if (user is null)
            {
                _logger.LogInformation("Forgot password requested for non-existent email: {Email}", normalizedEmail);
                return AuthResult.Success();
            }

            // Generate a cryptographically secure token
            var plaintextToken = GenerateSecureToken();
            var hashedToken = HashToken(plaintextToken);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(_emailOptions.ResetTokenExpirationMinutes);

            user.PasswordResetToken = hashedToken;
            user.PasswordResetExpiresAtUtc = expiresAtUtc;
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Build reset URL with token
            var resetUrl = $"{_emailOptions.ResetPasswordUrl}?token={WebUtility.UrlEncode(plaintextToken)}";

            try
            {
                var templateModel = new
                {
                    UserName = user.DisplayName ?? "User",
                    ResetUrl = resetUrl,
                    ExpirationMinutes = _emailOptions.ResetTokenExpirationMinutes,
                    AppName = "FindMyDevice",
                };

                var language = request.Language ?? user.Language ?? _emailOptions.DefaultLanguage;

                var emailBody = await _emailTemplateService.RenderAsync(
                    "PasswordReset",
                    templateModel,
                    language,
                    cancellationToken);

                await _emailService.SendEmailAsync(
                    user.Email!,
                    "Reset your password",
                    emailBody,
                    cancellationToken);

                _logger.LogInformation("Password reset email sent for user {UserId}.", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email for user {UserId}.", user.Id);
                return AuthResult.Fail(AuthError.EmailSendFailed, "Failed to send password reset email. Please try again later.");
            }

            return AuthResult.Success();
        }

        public async Task<AuthResult> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken)
        {
            var hashedToken = HashToken(request.Token);
            var user = await _userRepository.GetByPasswordResetTokenAsync(hashedToken, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Reset password failed: Invalid token.");
                return AuthResult.Fail(AuthError.InvalidResetToken, "Invalid or expired password reset token.");
            }

            if (!user.PasswordResetExpiresAtUtc.HasValue || user.PasswordResetExpiresAtUtc.Value < DateTime.UtcNow)
            {
                _logger.LogWarning("Reset password failed: Expired token.");
                return AuthResult.Fail(AuthError.ResetTokenExpired, "Invalid or expired password reset token.");
            }

            // Update password and clear reset token
            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetExpiresAtUtc = null;

            // Clear lockout if any, since they successfully reset password
            user.FailedAccessCount = 0;
            user.LockoutEndUtc = null;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset successfully for user {UserId}.", user.Id);
            return AuthResult.Success();
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        private async Task<AuthResult> GenerateAuthResponseAsync(User user, DateTime now, RefreshToken? existingRefreshToken, CancellationToken cancellationToken)
        {
            var refreshLifetime = TimeSpan.FromDays(_jwtOptions.RefreshTokenDays);
            var refreshResult = _refreshTokenService.Generate(user, now, now.Add(refreshLifetime), existingRefreshToken?.DeviceInfo);

            if (existingRefreshToken != null)
            {
                existingRefreshToken.RevokedAtUtc = now;
                existingRefreshToken.ReplacedByTokenId = refreshResult.Token.Id;
                await _refreshTokenRepository.UpdateAsync(existingRefreshToken, cancellationToken);
            }

            await _refreshTokenRepository.AddAsync(refreshResult.Token, cancellationToken);

            var accessResult = _jwtTokenService.GenerateAccessToken(user, now);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userDto = user.Adapt<DTOs.Users.UserDto>();

            var tokens = new AuthResponseDto
            {
                AccessToken = accessResult.Token,
                RefreshToken = refreshResult.PlaintextToken,
                ExpiresInSeconds = (int)(accessResult.ExpiresAtUtc - now).TotalSeconds,
                TokenType = "Bearer",
                User = userDto,
            };

            return AuthResult.Success(tokens);
        }

        private async Task<User> CreateUserAsync(string email, string? displayName, string? password, string? googleId, string? language, DateTime now, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                DisplayName = displayName?.Trim(),
                Language = language,
                CreatedAtUtc = now,
                FailedAccessCount = 0,
                GoogleId = googleId,
                LastLoginAtUtc = now,
            };

            if (!string.IsNullOrEmpty(password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, password);
            }

            await _userRepository.AddAsync(user, cancellationToken);
            return user;
        }

        private async Task SendWelcomeEmailAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                var language = user.Language ?? _emailOptions.DefaultLanguage;

                // Use configured AppUrl for dashboard link
                var dashboardUrl = $"{_emailOptions.AppUrl}/dashboard";

                var templateModel = new
                {
                    UserName = user.DisplayName ?? "User",
                    DashboardUrl = dashboardUrl,
                    AppName = "FindMyDevice",
                };

                var emailBody = await _emailTemplateService.RenderAsync(
                    "Welcome",
                    templateModel,
                    language,
                    cancellationToken);

                await _emailService.SendEmailAsync(
                    user.Email!,
                    "Welcome to FindMyDevice!",
                    emailBody,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Non-blocking error - we don't want to fail signup if email fails
                _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            }
        }

        private async Task UpdateUserLoginStatsAsync(User user, DateTime now, string? googleId, CancellationToken cancellationToken)
        {
            var needsUpdate = false;

            if (user.FailedAccessCount > 0)
            {
                user.FailedAccessCount = 0;
                needsUpdate = true;
            }

            if (user.LastLoginAtUtc != now)
            {
                user.LastLoginAtUtc = now;
                needsUpdate = true;
            }

            if (!string.IsNullOrEmpty(googleId) && user.GoogleId != googleId)
            {
                user.GoogleId = googleId;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await _userRepository.UpdateAsync(user, cancellationToken);
            }
        }
    }
}
