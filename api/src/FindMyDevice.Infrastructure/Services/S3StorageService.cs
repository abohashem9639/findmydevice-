using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FindMyDevice.Application.Configurations;
using FindMyDevice.Application.DTOs.Storage;
using FindMyDevice.Application.Helpers;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3StorageOptions _options;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IAmazonS3 s3Client, IOptions<S3StorageOptions> options, ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UploadUrlResponse> GeneratePresignedUploadUrlAsync(UploadUrlRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var ext = Path.GetExtension(request.FileName);
            var safeFileKey = request.Folder switch
            {
                StorageFolder.ProfileImages => $"profile-images/{userId}-{Guid.NewGuid()}{ext}",
                _ => $"users/{userId}/uploads/{Guid.NewGuid()}{ext}",
            };

            if (request.Folder == StorageFolder.ProfileImages && !MediaHelper.IsImageExtension(request.FileName))
            {
                throw new InvalidOperationException("Invalid file extension for profile image. Only images are allowed.");
            }

            var contentType = MediaHelper.GetMimeType(request.FileName);

            var s3Request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = safeFileKey,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Verb = HttpVerb.PUT,
                ContentType = contentType,
            };

            if (request.SizeBytes > 0)
            {
                s3Request.Headers["Content-Length"] = request.SizeBytes.ToString();
            }

            var url = _s3Client.GetPreSignedURL(s3Request);

            return new UploadUrlResponse { Url = url, FileKey = safeFileKey };
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned upload URL for folder {Folder}", request.Folder);
            throw;
        }
    }

    public Task<string> GenerateDownloadUrlAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var s3Request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = fileKey,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Verb = HttpVerb.GET,
            };

            var url = _s3Client.GetPreSignedURL(s3Request);
            return Task.FromResult(url);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL.");
            throw;
        }
    }

    public async Task<IDictionary<string, string>> GenerateBatchDownloadUrlsAsync(IEnumerable<string> fileKeys, CancellationToken cancellationToken = default)
    {
        var urls = new Dictionary<string, string>();

        try
        {
            foreach (var key in fileKeys)
            {
                cancellationToken.ThrowIfCancellationRequested();
                urls[key] = await GenerateDownloadUrlAsync(key, cancellationToken);
            }

            return urls;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error generating batch download URLs.");
            throw;
        }
    }
}
