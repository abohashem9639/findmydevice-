using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FindMyDevice.Application.Configurations;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailOptions> options, ILogger<SmtpEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken)
        {
            using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            if (!string.IsNullOrWhiteSpace(_options.SmtpUser) && !string.IsNullOrWhiteSpace(_options.SmtpPassword))
            {
                client.Credentials = new NetworkCredential(_options.SmtpUser, _options.SmtpPassword);
            }

            var fromAddress = new MailAddress(_options.FromEmail!, _options.FromName);
            var toAddress = new MailAddress(to);

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };

            try
            {
                await client.SendMailAsync(message, cancellationToken);
                _logger.LogInformation("Email sent successfully to {To} with subject '{Subject}'.", to, subject);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject '{Subject}'.", to, subject);
                throw;
            }
        }
    }
}
