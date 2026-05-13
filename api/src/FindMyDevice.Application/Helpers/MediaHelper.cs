namespace FindMyDevice.Application.Helpers
{
    public static class MediaHelper
    {
        public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4", ".mov", ".avi", ".mkv" };

        public static bool IsAllowedExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }

            return AllowedExtensions.Contains(ext.ToLowerInvariant());
        }

        public static bool IsImageExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp";
        }

        public static string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".mp4" => "video/mp4",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                ".mkv" => "video/x-matroska",
                _ => "application/octet-stream",
            };
        }
    }
}
