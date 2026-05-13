namespace FindMyDevice.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public required string TokenHash { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? RevokedAtUtc { get; set; }

        public Guid? ReplacedByTokenId { get; set; }

        public string? DeviceInfo { get; set; }

        public User? User { get; set; }
    }
}
