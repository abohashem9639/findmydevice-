using FindMyDevice.Application.DTOs.Auth;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Application.Interfaces
{
    public interface IJwtTokenService
    {
        AccessTokenResult GenerateAccessToken(User user, DateTime nowUtc);
    }
}
