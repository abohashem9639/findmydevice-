using AwesomeAssertions;
using FindMyDevice.Application.DTOs.Auth;
using FindMyDevice.Application.Validations.Auth;

namespace FindMyDevice.Application.UnitTests.Validations.Auth
{
    public class SigninRequestValidatorTests
    {
        private readonly SigninRequestValidator _validator;

        public SigninRequestValidatorTests()
        {
            _validator = new SigninRequestValidator();
        }

        [Fact]
        public void Validate_WithValidRequest_ShouldNotHaveErrors()
        {
            // Arrange
            var request = new SigninRequestDto
            {
                Email = "test@example.com",
                Password = "Password123!",
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_WithEmptyEmail_ShouldHaveValidationError(string? invalidEmail)
        {
            // Arrange
            var request = new SigninRequestDto
            {
                Email = invalidEmail!, // Disable nullable warning here for test
                Password = "Password123!",
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_WithInvalidEmailFormat_ShouldHaveValidationError()
        {
            // Arrange
            var request = new SigninRequestDto
            {
                Email = "invalid-email",
                Password = "Password123!",
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validate_WithEmptyPassword_ShouldHaveValidationError(string? invalidPassword)
        {
            // Arrange
            var request = new SigninRequestDto
            {
                Email = "test@example.com",
                Password = invalidPassword!, // Disable nullable warning here for test
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password");
        }
    }
}
