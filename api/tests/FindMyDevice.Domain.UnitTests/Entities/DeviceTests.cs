using AwesomeAssertions;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Domain.UnitTests.Entities
{
    public class DeviceTests
    {
        [Fact]
        public void Device_Initialization_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var device = new Device { Name = "Test Device" };

            // Assert
            device.Id.Should().Be(Guid.Empty);
            device.UserId.Should().Be(Guid.Empty);
            device.Platform.Should().Be("Android");
            device.FcmToken.Should().BeNull();
            device.LastSeenAtUtc.Should().BeNull();
        }

        [Fact]
        public void Device_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var device = new Device { Name = "هاتفي" };
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            // Act
            device.Id = id;
            device.UserId = userId;
            device.Manufacturer = "Xiaomi";
            device.Model = "Redmi Note 13";
            device.OsVersion = "14";
            device.FcmToken = "fcm-test-token";
            device.CreatedAtUtc = now;
            device.LastSeenAtUtc = now;

            // Assert
            device.Id.Should().Be(id);
            device.UserId.Should().Be(userId);
            device.Name.Should().Be("هاتفي");
            device.Manufacturer.Should().Be("Xiaomi");
            device.Model.Should().Be("Redmi Note 13");
            device.OsVersion.Should().Be("14");
            device.FcmToken.Should().Be("fcm-test-token");
            device.CreatedAtUtc.Should().Be(now);
            device.LastSeenAtUtc.Should().Be(now);
        }
    }
}
