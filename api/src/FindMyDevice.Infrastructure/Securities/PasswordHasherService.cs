using Microsoft.AspNetCore.Identity;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Infrastructure.Securities
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly IPasswordHasher<User> _passwordHasher;

        public PasswordHasherService(IPasswordHasher<User> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public string HashPassword(User user, string password) => _passwordHasher.HashPassword(user, password);

        public bool VerifyHashedPassword(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return false;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
