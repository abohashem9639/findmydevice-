using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using FindMyDevice.Application.DTOs.Auth;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;

        public GoogleAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GoogleTokenPayloadDto> VerifyGoogleTokenAsync(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { _configuration["Google:ClientId"]! },
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleTokenPayloadDto
            {
                Email = payload.Email,
                Name = payload.Name,
                Subject = payload.Subject,
                Issuer = payload.Issuer,
                Audience = payload.Audience?.ToString() ?? string.Empty,
            };
        }
    }
}
