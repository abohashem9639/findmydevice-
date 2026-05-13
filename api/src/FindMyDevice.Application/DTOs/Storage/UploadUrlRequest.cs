namespace FindMyDevice.Application.DTOs.Storage;

public class UploadUrlRequest
{
    public required string FileName { get; set; }

    public required string ContentType { get; set; }

    public long SizeBytes { get; set; }

    public required StorageFolder Folder { get; set; }
}
