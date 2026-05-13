namespace FindMyDevice.Application.DTOs.Auth
{
    public class AuthResult
    {
        private AuthResult(AuthResponseDto? tokens, AuthError error, string? errorDescription)
        {
            Tokens = tokens;
            Error = error;
            ErrorDescription = errorDescription;
        }

        public AuthResponseDto? Tokens { get; }

        public AuthError Error { get; }

        public string? ErrorDescription { get; }

        public bool Succeeded => Error == AuthError.None;

        public static AuthResult Success(AuthResponseDto? tokens = null) => new(tokens, AuthError.None, null);

        public static AuthResult Fail(AuthError error, string? errorDescription = null) => new(null, error, errorDescription);
    }
}
