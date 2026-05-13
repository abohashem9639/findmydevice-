using System.ComponentModel.DataAnnotations;

namespace FindMyDevice.Application.DTOs.Auth
{
    public class SigninWithGoogleRequestDto
    {
        [Required]
        public required string IdToken { get; set; }
    }
}
