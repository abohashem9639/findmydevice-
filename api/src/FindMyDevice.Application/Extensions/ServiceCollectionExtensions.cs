using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Application.Services;
using FindMyDevice.Application.Validations.Auth;

namespace FindMyDevice.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();

            services.AddValidatorsFromAssemblyContaining<SignupRequestValidator>();

            return services;
        }
    }
}
