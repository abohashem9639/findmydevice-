using AwesomeAssertions;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Domain.UnitTests.Entities
{
    public class UserTests
    {
        [Fact]
        public void User_Initialization_ShouldSetEmptyCollectionsAndDefaultValues()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            user.Id.Should().Be(Guid.Empty);
            user.FailedAccessCount.Should().Be(0);
        }

        [Fact]
        public void User_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var id = Guid.NewGuid();
            var email = "test@example.com";
            var now = DateTime.UtcNow;

            // Act
            user.Id = id;
            user.Email = email;
            user.CreatedAtUtc = now;

            // Assert
            user.Id.Should().Be(id);
            user.Email.Should().Be(email);
            user.CreatedAtUtc.Should().Be(now);
        }
    }
}
