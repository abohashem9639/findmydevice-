using System.Reflection;
using FindMyDevice.Application.DTOs;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.API.Providers
{
    public class ApplicationVersionProvider : IApplicationVersionProvider
    {
        private const string UnknownVersion = "0.0.0";
        private static readonly string ApplicationName = typeof(Program).Assembly.GetName().Name ?? "Unknown Application";
        private readonly IHostEnvironment _hostEnvironment;
        private readonly Assembly _assembly;

        public ApplicationVersionProvider(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _assembly = typeof(Program).Assembly;
        }

        public ApplicationVersionDto GetCurrent()
        {
            return new ApplicationVersionDto
            {
                Application = ApplicationName,
                Version = GetInformationalVersion(),
                CommitSha = GetAssemblyMetadata("CommitSha"),
                BuildTimestampUtc = GetAssemblyMetadata("BuildTimestampUtc"),
                Environment = _hostEnvironment.EnvironmentName,
            };
        }

        private string GetInformationalVersion()
        {
            return _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? _assembly.GetName().Version?.ToString()
                ?? UnknownVersion;
        }

        private string? GetAssemblyMetadata(string key)
        {
            return _assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(attribute => string.Equals(attribute.Key, key, StringComparison.Ordinal))
                ?.Value;
        }
    }
}
