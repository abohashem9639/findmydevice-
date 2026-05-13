using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FindMyDevice.Application.DTOs.Users;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.API.Controllers.V1
{
    [Authorize]
    [Route("api/v1/[controller]")]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public ActionResult<Guid> GetMe()
        {
            return Ok(UserId);
        }

        [HttpPatch("me/profile-image")]
        public async Task<ActionResult<UpdateProfileImageResponseDto>> UpdateProfileImage([FromBody] UpdateProfileImageDto dto, CancellationToken cancellationToken)
        {
            var response = await _userService.UpdateProfileImageAsync(UserId, dto, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id}/profile-image")]
        public async Task<ActionResult<ProfileImageUrlDto>> GetProfileImage(Guid id, [FromServices] IStorageService storageService, [FromServices] FindMyDevice.Application.Interfaces.IUserRepository userRepository, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null || string.IsNullOrEmpty(user.ProfileImage))
            {
                return NotFound();
            }

            var signedUrl = await storageService.GenerateDownloadUrlAsync(user.ProfileImage, cancellationToken);
            if (!string.IsNullOrEmpty(signedUrl))
            {
                return Ok(new ProfileImageUrlDto { Url = signedUrl });
            }

            return NotFound();
        }
    }
}
