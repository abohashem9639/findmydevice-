using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using FindMyDevice.Application.Configurations;
using FindMyDevice.Application.DTOs.Auth;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Infrastructure.Securities
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly SigningCredentials _signingCredentials;

        public JwtTokenService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;

            if (string.IsNullOrWhiteSpace(_jwtOptions.SigningKey))
            {
                throw new InvalidOperationException("Jwt:SigningKey is required.");
            }

            if (string.IsNullOrWhiteSpace(_jwtOptions.Issuer))
            {
                throw new InvalidOperationException("Jwt:Issuer is required.");
            }

            if (string.IsNullOrWhiteSpace(_jwtOptions.Audience))
            {
                throw new InvalidOperationException("Jwt:Audience is required.");
            }

            var signingKey = _jwtOptions.SigningKey;
            _signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256);
        }

        public AccessTokenResult GenerateAccessToken(User user, DateTime nowUtc)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("User email is required to generate an access token.");
            }

            var expiresAtUtc = nowUtc.AddMinutes(_jwtOptions.AccessTokenMinutes);
            var userEmail = user.Email;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, userEmail),
            };

            if (!string.IsNullOrWhiteSpace(user.DisplayName))
            {
                claims.Add(new Claim("name", user.DisplayName));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: nowUtc,
                expires: expiresAtUtc,
                signingCredentials: _signingCredentials);

            var tokenValue = _tokenHandler.WriteToken(token);
            return new AccessTokenResult(tokenValue, expiresAtUtc);
        }
    }
}
