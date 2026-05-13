using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FindMyDevice.Application.DTOs.Storage;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.API.Controllers.V1;

[Authorize]
[Route("api/v1/[controller]")]
public class StorageController : BaseApiController
{
    private readonly IStorageService _storageService;

    public StorageController(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpPost("upload-url")]
    public async Task<ActionResult<UploadUrlResponse>> GetUploadUrl([FromBody] UploadUrlRequest request, CancellationToken cancellationToken)
    {
        var response = await _storageService.GeneratePresignedUploadUrlAsync(request, UserId, cancellationToken);
        return Ok(response);
    }
}
