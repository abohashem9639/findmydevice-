namespace FindMyDevice.Application.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<DTOs.Auth.GoogleTokenPayloadDto> VerifyGoogleTokenAsync(string idToken);
    }
}
