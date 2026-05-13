using System.ComponentModel.DataAnnotations;

namespace FindMyDevice.Application.Configurations
{
    public class JwtOptions
    {
        [Required]
        public string? Issuer { get; set; }

        [Required]
        public string? Audience { get; set; }

        [Required]
        public string? SigningKey { get; set; }

        [Range(1, int.MaxValue)]
        public int AccessTokenMinutes { get; set; }

        [Range(1, int.MaxValue)]
        public int RefreshTokenDays { get; set; }
    }
}
