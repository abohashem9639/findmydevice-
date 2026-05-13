using FindMyDevice.Application.DTOs.Users;
using FindMyDevice.Application.Helpers;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateProfileImageResponseDto> UpdateProfileImageAsync(Guid userId, UpdateProfileImageDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (!ProfileImageKeyHelper.IsOwnedProfileImageKey(dto.FileKey, userId))
        {
            throw new InvalidOperationException("Profile image key is invalid for the current user.");
        }

        user.ProfileImage = dto.FileKey;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateProfileImageResponseDto
        {
            FileKey = dto.FileKey,
        };
    }
}
