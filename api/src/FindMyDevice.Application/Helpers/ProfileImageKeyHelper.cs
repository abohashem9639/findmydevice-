namespace FindMyDevice.Application.Helpers;

public static class ProfileImageKeyHelper
{
    public static bool IsOwnedProfileImageKey(string? fileKey, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            return false;
        }

        if (Uri.TryCreate(fileKey, UriKind.Absolute, out _))
        {
            return false;
        }

        var normalizedFileKey = fileKey.Replace('\\', '/');
        var expectedPrefix = $"profile-images/{userId:D}-";

        return normalizedFileKey.StartsWith(expectedPrefix, StringComparison.Ordinal)
            && MediaHelper.IsAllowedExtension(normalizedFileKey);
    }
}
