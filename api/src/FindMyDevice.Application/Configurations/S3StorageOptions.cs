namespace FindMyDevice.Application.Configurations;

public class S3StorageOptions
{
    public required string AccessKey { get; set; }

    public required string SecretKey { get; set; }

    public required string ServiceUrl { get; set; }

    public required string BucketName { get; set; }
}
