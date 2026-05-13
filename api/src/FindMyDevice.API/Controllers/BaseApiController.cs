using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FindMyDevice.API.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseApiController : ControllerBase
    {
        protected Guid UserId
        {
            get
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? User.FindFirstValue("id");
                if (Guid.TryParse(userIdString, out var userId))
                {
                    return userId;
                }

                throw new UnauthorizedAccessException("User ID not found in claims.");
            }
        }
    }
}
