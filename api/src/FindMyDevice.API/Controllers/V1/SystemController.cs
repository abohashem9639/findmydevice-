using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FindMyDevice.Application.DTOs;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.API.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [AllowAnonymous]
    public class SystemController : BaseApiController
    {
        private readonly IApplicationVersionProvider _applicationVersionProvider;

        public SystemController(IApplicationVersionProvider applicationVersionProvider)
        {
            _applicationVersionProvider = applicationVersionProvider;
        }

        [HttpGet("version")]
        [ProducesResponseType(typeof(ApplicationVersionDto), StatusCodes.Status200OK)]
        public ActionResult<ApplicationVersionDto> GetVersion(CancellationToken cancellationToken)
        {
            var version = _applicationVersionProvider.GetCurrent();
            return Ok(version);
        }
    }
}
