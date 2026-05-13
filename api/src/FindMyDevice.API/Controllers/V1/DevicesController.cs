using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FindMyDevice.Application.DTOs.Devices;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.API.Controllers.V1
{
    [Authorize]
    [Route("api/v1/[controller]")]
    public class DevicesController : BaseApiController
    {
        private readonly IDeviceService _deviceService;

        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DeviceDto>> Register([FromBody] RegisterDeviceRequestDto request, CancellationToken cancellationToken)
        {
            var device = await _deviceService.RegisterDeviceAsync(UserId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = device.Id }, device);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<DeviceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IReadOnlyList<DeviceDto>>> GetMine(CancellationToken cancellationToken)
        {
            var devices = await _deviceService.GetUserDevicesAsync(UserId, cancellationToken);
            return Ok(devices);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeviceDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var device = await _deviceService.GetDeviceAsync(UserId, id, cancellationToken);
            return device is null ? NotFound() : Ok(device);
        }

        [HttpPatch("{id:guid}/fcm-token")]
        [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeviceDto>> UpdateFcmToken([FromRoute] Guid id, [FromBody] UpdateFcmTokenRequestDto request, CancellationToken cancellationToken)
        {
            var device = await _deviceService.UpdateFcmTokenAsync(UserId, id, request.FcmToken, cancellationToken);
            return device is null ? NotFound() : Ok(device);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _deviceService.DeleteDeviceAsync(UserId, id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("{id:guid}/heartbeat")]
        [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeviceDto>> Heartbeat(
            [FromRoute] Guid id,
            [FromBody] HeartbeatRequestDto request,
            CancellationToken cancellationToken)
        {
            var device = await _deviceService.RecordHeartbeatAsync(UserId, id, request, cancellationToken);
            return device is null ? NotFound() : Ok(device);
        }
    }
}
