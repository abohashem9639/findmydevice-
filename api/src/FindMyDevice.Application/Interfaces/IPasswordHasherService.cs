using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Application.Interfaces
{
    public interface IPasswordHasherService
    {
        string HashPassword(User user, string password);

        bool VerifyHashedPassword(User user, string password);
    }
}
