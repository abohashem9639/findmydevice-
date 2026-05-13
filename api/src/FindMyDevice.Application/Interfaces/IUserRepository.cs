using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task AddAsync(User user, CancellationToken cancellationToken);

        Task UpdateAsync(User user, CancellationToken cancellationToken);

        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

        Task<User?> GetByPasswordResetTokenAsync(string tokenHash, CancellationToken cancellationToken);
    }
}
