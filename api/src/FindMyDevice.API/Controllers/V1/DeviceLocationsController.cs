using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FindMyDevice.Application.DTOs.DeviceLocations;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.API.Controllers.V1
{
    [Authorize]
    [Route("api/v1/Devices/{deviceId:guid}/Locations")]
    public class DeviceLocationsController : BaseApiController
    {
        private const int DefaultLimit = 100;
        private const int MaxLimit = 500;

        private readonly IDeviceLocationService _locationService;

        public DeviceLocationsController(IDeviceLocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(LocationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LocationDto>> Report(
            [FromRoute] Guid deviceId,
            [FromBody] ReportLocationRequestDto request,
            CancellationToken cancellationToken)
        {
            var location = await _locationService.ReportLocationAsync(UserId, deviceId, request, cancellationToken);
            if (location is null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetLatest), new { deviceId }, location);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<LocationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IReadOnlyList<LocationDto>>> GetList(
            [FromRoute] Guid deviceId,
            [FromQuery] int limit = DefaultLimit,
            CancellationToken cancellationToken = default)
        {
            var safeLimit = Math.Clamp(limit, 1, MaxLimit);
            var locations = await _locationService.GetLocationsAsync(UserId, deviceId, safeLimit, cancellationToken);
            return Ok(locations);
        }

        [HttpGet("latest")]
        [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LocationDto>> GetLatest(
            [FromRoute] Guid deviceId,
            CancellationToken cancellationToken)
        {
            var location = await _locationService.GetLatestLocationAsync(UserId, deviceId, cancellationToken);
            return location is null ? NoContent() : Ok(location);
        }
    }
}
