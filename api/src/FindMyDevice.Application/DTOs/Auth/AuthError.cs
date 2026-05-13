namespace FindMyDevice.Application.DTOs.Auth
{
    public enum AuthError
    {
        None = 0,
        DuplicateEmail,
        InvalidCredentials,
        InvalidRefreshToken,
        UserLockedOut,
        InvalidResetToken,
        ResetTokenExpired,
        EmailSendFailed,
    }
}
