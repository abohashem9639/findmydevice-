using System.ComponentModel.DataAnnotations;

namespace FindMyDevice.Application.Configurations
{
    public class EmailOptions
    {
        [Required]
        public string? SmtpHost { get; set; }

        [Required]
        [Range(1, 65535)]
        public int SmtpPort { get; set; }

        [Required]
        [EmailAddress]
        public string? FromEmail { get; set; }

        [Required]
        public string? FromName { get; set; }

        public string? SmtpUser { get; set; }

        public string? SmtpPassword { get; set; }

        [Required]
        public bool EnableSsl { get; set; }

        [Required]
        [Url]
        public string ResetPasswordUrl { get; set; } = string.Empty;

        [Required]
        [Url]
        public string AppUrl { get; set; } = string.Empty;

        [Required]
        [Range(1, 1440)]
        public int ResetTokenExpirationMinutes { get; set; }

        [Required]
        public string? DefaultLanguage { get; set; }
    }
}
