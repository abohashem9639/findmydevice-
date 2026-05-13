namespace FindMyDevice.Application.DTOs.Auth
{
    public class SigninRequestDto
    {
        public required string Email { get; set; }

        public required string Password { get; set; }
    }
}
