namespace FindMyDevice.Application.Extensions
{
    public static class StringExtensions
    {
        public static string NormalizeEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return string.Empty;
            }

            return email.Trim().ToLowerInvariant();
        }
    }
}
