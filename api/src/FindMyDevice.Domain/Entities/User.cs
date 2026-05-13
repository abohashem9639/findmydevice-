namespace FindMyDevice.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string? Email { get; set; }

        public string? PasswordHash { get; set; }

        public string? DisplayName { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? LastLoginAtUtc { get; set; }

        public int FailedAccessCount { get; set; }

        public DateTime? LockoutEndUtc { get; set; }

        public string? GoogleId { get; set; }

        public string? Language { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetExpiresAtUtc { get; set; }

        public string? ProfileImage { get; set; }
    }
}
