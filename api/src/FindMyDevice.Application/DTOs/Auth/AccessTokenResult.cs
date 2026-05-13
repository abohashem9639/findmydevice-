namespace FindMyDevice.Application.DTOs.Auth
{
    public class AccessTokenResult
    {
        public AccessTokenResult(string token, DateTime expiresAtUtc)
        {
            Token = token;
            ExpiresAtUtc = expiresAtUtc;
        }

        public string Token { get; }

        public DateTime ExpiresAtUtc { get; }
    }
}
