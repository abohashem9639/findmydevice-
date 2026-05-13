namespace FindMyDevice.Application.DTOs.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public required string Email { get; set; }

        public string? DisplayName { get; set; }

        public string? ProfileImage { get; set; }
    }
}
