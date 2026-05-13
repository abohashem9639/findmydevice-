using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;
using FindMyDevice.Infrastructure.Persistence;
using FindMyDevice.Infrastructure.Persistence.Contexts;
using FindMyDevice.Infrastructure.Persistence.Repositories;
using FindMyDevice.Infrastructure.Securities;
using FindMyDevice.Infrastructure.Services;
using Amazon.S3;
using FindMyDevice.Application.Configurations;
using FindMyDevice.Application.Services;

namespace FindMyDevice.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FindMyDeviceDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("Default"))
                    .UseSnakeCaseNamingConvention();
            });

            services.AddRepositories();
            services.AddServices();
            services.AddS3Storage(configuration);

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<IDeviceLocationRepository, DeviceLocationRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IPasswordHasherService, PasswordHasherService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IDeviceLocationService, DeviceLocationService>();

            return services;
        }

        private static IServiceCollection AddS3Storage(this IServiceCollection services, IConfiguration configuration)
        {
            var s3Options = configuration.GetSection("S3Storage").Get<S3StorageOptions>();
            if (s3Options != null)
            {
                var s3Config = new AmazonS3Config
                {
                    ServiceURL = s3Options.ServiceUrl,
                    ForcePathStyle = true,
                };

                var s3Credentials = new Amazon.Runtime.BasicAWSCredentials(s3Options.AccessKey, s3Options.SecretKey);
                // Register the S3 client as a Singleton. AWS SDK clients are thread-safe
                // and designed to be reused across requests to optimize HTTP connection pooling.
                services.AddSingleton<IAmazonS3>(new AmazonS3Client(s3Credentials, s3Config));
                services.AddScoped<IStorageService, S3StorageService>();
            }

            return services;
        }
    }
}
