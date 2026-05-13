namespace FindMyDevice.Application.DTOs.Auth
{
    public class GoogleTokenPayloadDto
    {
        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;
    }
}
