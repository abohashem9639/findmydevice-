using System.Net;
using System.Net.Http.Headers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.API.IntegrationTests.Controllers
{
    public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public UsersControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetMe_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/users/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMe_WithValidToken_ShouldReturnUserId()
        {
            // Arrange
            var client = _factory.CreateClient();
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "test@example.com" };

            // Resolve JwtTokenService and JwtOptions from the DI container to generate a real token
            using var scope = _factory.Services.CreateScope();
            var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

            var tokenResult = jwtTokenService.GenerateAccessToken(user, DateTime.UtcNow);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            // Act
            var response = await client.GetAsync("/api/v1/users/me");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Be($"\"{userId}\""); // JSON strings include quotes
        }
    }
}
