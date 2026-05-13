using FindMyDevice.Application.DTOs.Auth;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Application.Interfaces
{
    public interface IRefreshTokenService
    {
        RefreshTokenGenerationResult Generate(User user, DateTime nowUtc, DateTime expiresAtUtc, string? deviceInfo);

        string HashToken(string token);
    }
}
