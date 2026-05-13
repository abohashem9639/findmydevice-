using System.Security.Cryptography;
using System.Text;
using FindMyDevice.Application.DTOs.Auth;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Infrastructure.Securities
{
    public class RefreshTokenService : IRefreshTokenService
    {
        public RefreshTokenGenerationResult Generate(User user, DateTime nowUtc, DateTime expiresAtUtc, string? deviceInfo)
        {
            var plaintext = GenerateSecureToken();
            var tokenHash = HashToken(plaintext);

            var token = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAtUtc = nowUtc,
                ExpiresAtUtc = expiresAtUtc,
                DeviceInfo = deviceInfo,
            };

            return new RefreshTokenGenerationResult(token, plaintext);
        }

        public string HashToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = SHA256.HashData(tokenBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }
    }
}
