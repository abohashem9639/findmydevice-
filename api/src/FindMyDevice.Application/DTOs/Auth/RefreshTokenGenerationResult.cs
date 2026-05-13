using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Application.DTOs.Auth
{
    public class RefreshTokenGenerationResult
    {
        public RefreshTokenGenerationResult(RefreshToken token, string plaintextToken)
        {
            Token = token;
            PlaintextToken = plaintextToken;
        }

        public RefreshToken Token { get; }

        public string PlaintextToken { get; }
    }
}
