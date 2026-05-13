using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FindMyDevice.Application.DTOs.Auth;
using FindMyDevice.Application.Interfaces;

namespace FindMyDevice.API.Controllers.V1
{
    [Route("api/v1/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AuthResponseDto>> Signup([FromBody] SignupRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.SignupAsync(request, cancellationToken);
            if (result.Succeeded && result.Tokens is not null)
            {
                return Ok(result.Tokens);
            }

            return Problem(title: result.Error.ToString(), detail: result.ErrorDescription, statusCode: GetStatusCodeForError(result.Error));
        }

        [HttpPost("signin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status423Locked)]
        public async Task<ActionResult<AuthResponseDto>> Signin([FromBody] SigninRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.SigninAsync(request, cancellationToken);
            if (result.Succeeded && result.Tokens is not null)
            {
                return Ok(result.Tokens);
            }

            return Problem(title: result.Error.ToString(), detail: result.ErrorDescription, statusCode: GetStatusCodeForError(result.Error));
        }

        [HttpPost("google")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status423Locked)]
        public async Task<ActionResult<AuthResponseDto>> SigninWithGoogle([FromBody] SigninWithGoogleRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.SigninWithGoogleAsync(request, cancellationToken);
            if (result.Succeeded && result.Tokens is not null)
            {
                return Ok(result.Tokens);
            }

            return Problem(title: result.Error.ToString(), detail: result.ErrorDescription, statusCode: GetStatusCodeForError(result.Error));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshAsync(request, cancellationToken);
            if (result.Succeeded && result.Tokens is not null)
            {
                return Ok(result.Tokens);
            }

            return Problem(title: result.Error.ToString(), detail: result.ErrorDescription, statusCode: GetStatusCodeForError(result.Error));
        }

        [HttpPost("revoke")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.RevokeAsync(request, cancellationToken);
            if (result.Error == AuthError.None)
            {
                return NoContent();
            }

            return Problem(title: result.Error.ToString(), detail: result.ErrorDescription, statusCode: GetStatusCodeForError(result.Error));
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.ForgotPasswordAsync(request, cancellationToken);
            if (result.Succeeded)
            {
                return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
            }

            return Problem(title: result.Error.ToString(), detail: result.ErrorDescription, statusCode: GetStatusCodeForError(result.Error));
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.ResetPasswordAsync(request, cancellationToken);
            if (result.Succeeded)
            {
                return Ok(new { message = "Password has been reset successfully. You can now sign in with your new password." });
            }

            return Problem(title: result.Error.ToString(), detail: result.ErrorDescription, statusCode: GetStatusCodeForError(result.Error));
        }

        private int GetStatusCodeForError(AuthError error)
        {
            return error switch
            {
                AuthError.DuplicateEmail => StatusCodes.Status409Conflict,
                AuthError.InvalidCredentials => StatusCodes.Status401Unauthorized,
                AuthError.UserLockedOut => StatusCodes.Status423Locked,
                AuthError.InvalidRefreshToken => StatusCodes.Status401Unauthorized,
                AuthError.InvalidResetToken => StatusCodes.Status401Unauthorized,
                AuthError.ResetTokenExpired => StatusCodes.Status401Unauthorized,
                AuthError.EmailSendFailed => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status400BadRequest,
            };
        }
    }
}
