namespace FindMyDevice.Application.DTOs.Auth
{
    public class ForgotPasswordRequestDto
    {
        public required string Email { get; set; }

        public string? Language { get; set; }
    }
}
