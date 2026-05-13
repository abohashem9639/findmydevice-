namespace FindMyDevice.Application.DTOs.Auth
{
    public class ResetPasswordRequestDto
    {
        public required string Token { get; set; }

        public required string NewPassword { get; set; }

        public required string ConfirmPassword { get; set; }
    }
}
