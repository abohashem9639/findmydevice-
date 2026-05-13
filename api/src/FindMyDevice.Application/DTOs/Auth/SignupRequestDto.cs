namespace FindMyDevice.Application.DTOs.Auth
{
    public class SignupRequestDto
    {
        public required string Email { get; set; }

        public required string Password { get; set; }

        public required string ConfirmPassword { get; set; }

        public string? DisplayName { get; set; }

        public string? Language { get; set; }
    }
}
