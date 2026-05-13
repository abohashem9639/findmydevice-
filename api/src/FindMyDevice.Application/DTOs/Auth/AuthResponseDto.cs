using FindMyDevice.Application.DTOs.Users;

namespace FindMyDevice.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }

        public int ExpiresInSeconds { get; set; }

        public string? TokenType { get; set; }

        public UserDto? User { get; set; }
    }
}
