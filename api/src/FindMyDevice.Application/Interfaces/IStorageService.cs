using FindMyDevice.Application.DTOs.Storage;

namespace FindMyDevice.Application.Interfaces;

public interface IStorageService
{
    Task<UploadUrlResponse> GeneratePresignedUploadUrlAsync(UploadUrlRequest request, Guid userId, CancellationToken cancellationToken = default);

    Task<string> GenerateDownloadUrlAsync(string fileKey, CancellationToken cancellationToken = default);

    Task<IDictionary<string, string>> GenerateBatchDownloadUrlsAsync(IEnumerable<string> fileKeys, CancellationToken cancellationToken = default);
}
