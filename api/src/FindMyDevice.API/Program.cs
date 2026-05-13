using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using FindMyDevice.Application.Configurations;
using FindMyDevice.Application.Extensions;
using FindMyDevice.Infrastructure.Extensions;

namespace FindMyDevice.API
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddOptions<JwtOptions>()
                .Bind(builder.Configuration.GetSection("Jwt"))
                .ValidateDataAnnotations()
                .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey), "Jwt:SigningKey is required.")
                .Validate(options => options.AccessTokenMinutes > 0 && options.RefreshTokenDays > 0, "Jwt durations must be greater than zero.")
                .ValidateOnStart();

            // Email and S3 are optional features in this deployment — bind them
            // for any code that does opt in, but don't enforce validation at
            // startup so the API can run without SMTP / S3 credentials.
            builder.Services.AddOptions<EmailOptions>()
                .Bind(builder.Configuration.GetSection("Email"));

            builder.Services.AddOptions<S3StorageOptions>()
                .Bind(builder.Configuration.GetSection("S3Storage"));

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddSingleton<FindMyDevice.Application.Interfaces.IApplicationVersionProvider, FindMyDevice.API.Providers.ApplicationVersionProvider>();

            builder.Services.AddExceptionHandler<FindMyDevice.API.Middlewares.GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Components ??= new();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                    var bearerScheme = new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter: Bearer {token}",
                    };
                    document.Components.SecuritySchemes["Bearer"] = bearerScheme;

                    document.Security ??= new List<OpenApiSecurityRequirement>();

                    var bearerReference = new OpenApiSecuritySchemeReference("Bearer", document, null);

                    var requirement = new OpenApiSecurityRequirement
                    {
                        { bearerReference, new List<string>() },
                    };

                    document.Security.Add(requirement);

                    return Task.CompletedTask;
                });
            });

            var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
            if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
                string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
                string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
            {
                throw new InvalidOperationException("JWT options are not configured. Please set Jwt:Issuer, Jwt:Audience, and Jwt:SigningKey.");
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = signingKey,
                        ClockSkew = TimeSpan.FromMinutes(1),
                    };
                });

            builder.Services.AddCors(options =>
            {
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                options.AddPolicy("AllowWeb", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.AddAuthorization();

            // Configure forwarded headers to ensure the app correctly resolves the client's IP and the original request scheme (HTTPS).
            // This is primarily required so that OpenAPI/Scalar can generate the correct base URLs (using https://)
            // when the .NET app is running behind a reverse proxy (like Caddy) inside a Docker container.
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                // We specifically only need 'X-Forwarded-For' (client IP) and 'X-Forwarded-Proto' (http vs https).
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // Clear existing proxies to trust the Docker internal network proxy (Caddy)
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var app = builder.Build();

            app.UseForwardedHeaders();

            app.MapScalarApiReference();
            app.MapOpenApi();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseExceptionHandler();

            app.UseCors("AllowWeb");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
