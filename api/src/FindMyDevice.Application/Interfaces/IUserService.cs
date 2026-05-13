using FindMyDevice.Application.DTOs.Users;

namespace FindMyDevice.Application.Interfaces;

public interface IUserService
{
    Task<UpdateProfileImageResponseDto> UpdateProfileImageAsync(Guid userId, UpdateProfileImageDto dto, CancellationToken cancellationToken = default);
}
