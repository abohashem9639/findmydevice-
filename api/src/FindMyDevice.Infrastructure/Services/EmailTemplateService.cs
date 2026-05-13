using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scriban;
using Scriban.Runtime;
using FindMyDevice.Application.Configurations;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<EmailTemplateService> _logger;
        private readonly Assembly _assembly;
        private readonly string _resourcePrefix;

        public EmailTemplateService(IOptions<EmailOptions> options, ILogger<EmailTemplateService> logger)
        {
            _options = options.Value;
            _logger = logger;
            _assembly = typeof(EmailTemplateService).Assembly;
            _resourcePrefix = "FindMyDevice.Infrastructure.Templates.Email";
        }

        public async Task<string> RenderAsync(string templateName, object model, string? language = null, CancellationToken cancellationToken = default)
        {
            var effectiveLanguage = language ?? _options.DefaultLanguage ?? "en";
            var templateContent = await LoadTemplateAsync(templateName, effectiveLanguage);

            if (string.IsNullOrEmpty(templateContent))
            {
                throw new InvalidOperationException($"Email template '{templateName}' not found for language '{effectiveLanguage}' or fallback.");
            }

            var template = Template.Parse(templateContent);
            if (template.HasErrors)
            {
                var errors = string.Join(", ", template.Messages.Select(m => m.Message));
                _logger.LogError("Template parsing errors in {TemplateName}: {Errors}", templateName, errors);
                throw new InvalidOperationException($"Template parsing failed: {errors}");
            }

            var scriptObject = new ScriptObject();
            scriptObject.Import(model, renamer: member => member.Name.ToLowerInvariant().Replace("_", string.Empty) switch
            {
                var name => ToSnakeCase(member.Name),
            });

            var context = new TemplateContext { MemberRenamer = member => ToSnakeCase(member.Name) };
            context.PushGlobal(scriptObject);

            return await template.RenderAsync(context);
        }

        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var result = new System.Text.StringBuilder();
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        result.Append('_');
                    }

                    result.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        private async Task<string?> LoadTemplateAsync(string templateName, string language)
        {
            // Try exact language match (e.g., "en-US")
            var content = await TryLoadResourceAsync(templateName, language);
            if (content != null)
            {
                return content;
            }

            // Try base language (e.g., "en" from "en-US")
            if (language.Contains('-'))
            {
                var baseLanguage = language.Split('-')[0];
                content = await TryLoadResourceAsync(templateName, baseLanguage);
                if (content != null)
                {
                    return content;
                }
            }

            // Fall back to default language
            if (language != "en")
            {
                content = await TryLoadResourceAsync(templateName, "en");
                if (content != null)
                {
                    _logger.LogDebug("Falling back to English for template {TemplateName}, requested language: {Language}", templateName, language);
                    return content;
                }
            }

            return null;
        }

        private async Task<string?> TryLoadResourceAsync(string templateName, string language)
        {
            var resourceName = $"{_resourcePrefix}.{language}.{templateName}.html";
            using var stream = _assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                return null;
            }

            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
