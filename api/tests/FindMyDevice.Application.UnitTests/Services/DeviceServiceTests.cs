using AwesomeAssertions;
using FakeItEasy;
using FindMyDevice.Application.DTOs.Devices;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Application.Services;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Application.UnitTests.Services
{
    public class DeviceServiceTests
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DeviceService _service;

        public DeviceServiceTests()
        {
            _deviceRepository = A.Fake<IDeviceRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _service = new DeviceService(_deviceRepository, _unitOfWork);
        }

        // ─────────────────────────── RegisterDeviceAsync ───────────────────────────

        [Fact]
        public async Task RegisterDeviceAsync_WithValidRequest_ShouldAddAndSave()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new RegisterDeviceRequestDto
            {
                Name = "هاتفي",
                Manufacturer = "Xiaomi",
                Model = "Redmi Note 13",
                Platform = "Android",
                OsVersion = "14",
                FcmToken = "token-abc",
            };

            // Act
            var result = await _service.RegisterDeviceAsync(userId, request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("هاتفي");
            result.Manufacturer.Should().Be("Xiaomi");
            result.Platform.Should().Be("Android");
            result.HasFcmToken.Should().BeTrue();
            result.IsOnline.Should().BeFalse(); // LastSeenAtUtc is null at registration
            A.CallTo(() => _deviceRepository.AddAsync(A<Device>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task RegisterDeviceAsync_WithMissingPlatform_ShouldDefaultToAndroid()
        {
            // Arrange
            var request = new RegisterDeviceRequestDto { Name = "Test", Platform = null };

            // Act
            var result = await _service.RegisterDeviceAsync(Guid.NewGuid(), request);

            // Assert
            result.Platform.Should().Be("Android");
        }

        [Fact]
        public async Task RegisterDeviceAsync_WithBlankOptionalFields_ShouldNormalizeToNull()
        {
            // Arrange
            var request = new RegisterDeviceRequestDto
            {
                Name = "Test",
                Manufacturer = "   ",
                Model = string.Empty,
                FcmToken = "  ",
            };

            // Act
            var result = await _service.RegisterDeviceAsync(Guid.NewGuid(), request);

            // Assert
            result.Manufacturer.Should().BeNull();
            result.Model.Should().BeNull();
            result.HasFcmToken.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterDeviceAsync_ShouldTrimName()
        {
            // Arrange
            var request = new RegisterDeviceRequestDto { Name = "  My Device  " };

            // Act
            var result = await _service.RegisterDeviceAsync(Guid.NewGuid(), request);

            // Assert
            result.Name.Should().Be("My Device");
        }

        // ─────────────────────────── GetUserDevicesAsync ───────────────────────────

        [Fact]
        public async Task GetUserDevicesAsync_WhenNoDevices_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            A.CallTo(() => _deviceRepository.GetByUserIdAsync(userId, A<CancellationToken>._))
                .Returns(Array.Empty<Device>());

            // Act
            var result = await _service.GetUserDevicesAsync(userId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserDevicesAsync_ShouldReturnMappedDevices()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var devices = new[]
            {
                new Device { Id = Guid.NewGuid(), UserId = userId, Name = "A", Platform = "Android", CreatedAtUtc = DateTime.UtcNow },
                new Device { Id = Guid.NewGuid(), UserId = userId, Name = "B", Platform = "iOS", CreatedAtUtc = DateTime.UtcNow },
            };
            A.CallTo(() => _deviceRepository.GetByUserIdAsync(userId, A<CancellationToken>._)).Returns(devices);

            // Act
            var result = await _service.GetUserDevicesAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Select(d => d.Name).Should().Contain(["A", "B"]);
        }

        // ─────────────────────────── GetDeviceAsync ───────────────────────────

        [Fact]
        public async Task GetDeviceAsync_WhenNotFound_ShouldReturnNull()
        {
            // Arrange
            A.CallTo(() => _deviceRepository.GetByIdForUserAsync(A<Guid>._, A<Guid>._, A<CancellationToken>._))
                .Returns((Device?)null);

            // Act
            var result = await _service.GetDeviceAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDeviceAsync_WhenFound_ShouldReturnMappedDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            var device = new Device
            {
                Id = deviceId,
                UserId = userId,
                Name = "Phone",
                Platform = "Android",
                FcmToken = "token-xyz",
                CreatedAtUtc = DateTime.UtcNow,
            };
            A.CallTo(() => _deviceRepository.GetByIdForUserAsync(deviceId, userId, A<CancellationToken>._)).Returns(device);

            // Act
            var result = await _service.GetDeviceAsync(userId, deviceId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(deviceId);
            result.Name.Should().Be("Phone");
            result.HasFcmToken.Should().BeTrue();
        }

        // ─────────────────────────── UpdateFcmTokenAsync ───────────────────────────

        [Fact]
        public async Task UpdateFcmTokenAsync_WhenNotFound_ShouldReturnNullAndNotSave()
        {
            // Arrange
            A.CallTo(() => _deviceRepository.GetByIdForUserAsync(A<Guid>._, A<Guid>._, A<CancellationToken>._))
                .Returns((Device?)null);

            // Act
            var result = await _service.UpdateFcmTokenAsync(Guid.NewGuid(), Guid.NewGuid(), "new-token");

            // Assert
            result.Should().BeNull();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task UpdateFcmTokenAsync_WhenFound_ShouldUpdateAndSave()
        {
            // Arrange
            var device = new Device
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Phone",
                Platform = "Android",
                FcmToken = "old-token",
                CreatedAtUtc = DateTime.UtcNow,
            };
            A.CallTo(() => _deviceRepository.GetByIdForUserAsync(device.Id, device.UserId, A<CancellationToken>._))
                .Returns(device);

            // Act
            var result = await _service.UpdateFcmTokenAsync(device.UserId, device.Id, "new-token-123");

            // Assert
            result.Should().NotBeNull();
            device.FcmToken.Should().Be("new-token-123");
            A.CallTo(() => _deviceRepository.UpdateAsync(device, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        // ─────────────────────────── DeleteDeviceAsync ───────────────────────────

        [Fact]
        public async Task DeleteDeviceAsync_WhenNotFound_ShouldReturnFalseAndNotSave()
        {
            // Arrange
            A.CallTo(() => _deviceRepository.GetByIdForUserAsync(A<Guid>._, A<Guid>._, A<CancellationToken>._))
                .Returns((Device?)null);

            // Act
            var result = await _service.DeleteDeviceAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task DeleteDeviceAsync_WhenFound_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var device = new Device
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Phone",
                Platform = "Android",
                CreatedAtUtc = DateTime.UtcNow,
            };
            A.CallTo(() => _deviceRepository.GetByIdForUserAsync(device.Id, device.UserId, A<CancellationToken>._))
                .Returns(device);

            // Act
            var result = await _service.DeleteDeviceAsync(device.UserId, device.Id);

            // Assert
            result.Should().BeTrue();
            A.CallTo(() => _deviceRepository.DeleteAsync(device, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        // ─────────────────────────── IsOnline computation ───────────────────────────

        [Fact]
        public async Task GetDeviceAsync_WithRecentLastSeen_ShouldBeOnline()
        {
            // Arrange
            var device = new Device
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Phone",
                Platform = "Android",
                LastSeenAtUtc = DateTime.UtcNow.AddSeconds(-30),
                CreatedAtUtc = DateTime.UtcNow,
            };
            A.CallTo(() => _deviceRepository.GetByIdForUserAsync(device.Id, device.UserId, A<CancellationToken>._))
                .Returns(device);

            // Act
            var result = await _service.GetDeviceAsync(device.UserId, device.Id);

            // Assert
            result!.IsOnline.Should().BeTrue();
        }

        [Fact]
        public async Task GetDeviceAsync_WithStaleLastSeen_ShouldBeOffline()
        {
            // Arrange
            var device = new Device
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Phone",
                Platform = "Android",
                LastSeenAtUtc = DateTime.UtcNow.AddMinutes(-10),
                CreatedAtUtc = DateTime.UtcNow,
            };
            A.CallTo(() => _deviceRepository.GetByIdForUserAsync(device.Id, device.UserId, A<CancellationToken>._))
                .Returns(device);

            // Act
            var result = await _service.GetDeviceAsync(device.UserId, device.Id);

            // Assert
            result!.IsOnline.Should().BeFalse();
        }

        [Fact]
        public async Task GetDeviceAsync_WithNullLastSeen_ShouldBeOffline()
        {
            // Arrange
            var device = new Device
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Phone",
                Platform = "Android",
                LastSeenAtUtc = null,
                CreatedAtUtc = DateTime.UtcNow,
            };
            A.CallTo(() => _deviceRepository.GetByIdForUserAsync(device.Id, device.UserId, A<CancellationToken>._))
                .Returns(device);

            // Act
            var result = await _service.GetDeviceAsync(device.UserId, device.Id);

            // Assert
            result!.IsOnline.Should().BeFalse();
        }
    }
}
