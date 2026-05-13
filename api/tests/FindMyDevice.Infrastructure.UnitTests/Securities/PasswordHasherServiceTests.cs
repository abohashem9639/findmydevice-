using AwesomeAssertions;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using FindMyDevice.Domain.Entities;
using FindMyDevice.Infrastructure.Securities;

namespace FindMyDevice.Infrastructure.UnitTests.Securities
{
    public class PasswordHasherServiceTests
    {
        private readonly IPasswordHasher<User> _passwordHasherFake;
        private readonly PasswordHasherService _service;

        public PasswordHasherServiceTests()
        {
            _passwordHasherFake = A.Fake<IPasswordHasher<User>>();
            _service = new PasswordHasherService(_passwordHasherFake);
        }

        [Fact]
        public void HashPassword_ShouldReturnHashedResultFromInnerHasher()
        {
            // Arrange
            var user = new User();
            var password = "password123";
            var expectedHash = "hashed_password";

            A.CallTo(() => _passwordHasherFake.HashPassword(user, password)).Returns(expectedHash);

            // Act
            var result = _service.HashPassword(user, password);

            // Assert
            result.Should().Be(expectedHash);
            A.CallTo(() => _passwordHasherFake.HashPassword(user, password)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void VerifyHashedPassword_WithNullOrEmptyHash_ShouldReturnFalse()
        {
            // Arrange
            var user = new User { PasswordHash = null };
            var password = "password123";

            // Act
            var result = _service.VerifyHashedPassword(user, password);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(PasswordVerificationResult.Success)]
        [InlineData(PasswordVerificationResult.SuccessRehashNeeded)]
        public void VerifyHashedPassword_WithSuccessfulVerification_ShouldReturnTrue(PasswordVerificationResult verificationResult)
        {
            // Arrange
            var user = new User { PasswordHash = "existing_hash" };
            var password = "password123";

            A.CallTo(() => _passwordHasherFake.VerifyHashedPassword(user, user.PasswordHash, password)).Returns(verificationResult);

            // Act
            var result = _service.VerifyHashedPassword(user, password);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyHashedPassword_WithFailedVerification_ShouldReturnFalse()
        {
            // Arrange
            var user = new User { PasswordHash = "existing_hash" };
            var password = "wrong_password";

            A.CallTo(() => _passwordHasherFake.VerifyHashedPassword(user, user.PasswordHash, password)).Returns(PasswordVerificationResult.Failed);

            // Act
            var result = _service.VerifyHashedPassword(user, password);

            // Assert
            result.Should().BeFalse();
        }
    }
}
