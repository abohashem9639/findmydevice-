using Microsoft.EntityFrameworkCore;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;
using FindMyDevice.Infrastructure.Persistence.Contexts;

namespace FindMyDevice.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly FindMyDeviceDbContext _dbContext;

        public UserRepository(FindMyDeviceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

        public async Task AddAsync(User user, CancellationToken cancellationToken)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken)
        {
            _dbContext.Users.Update(user);
            return Task.CompletedTask;
        }

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
            _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);

        public Task<User?> GetByPasswordResetTokenAsync(string tokenHash, CancellationToken cancellationToken) =>
            _dbContext.Users.SingleOrDefaultAsync(u => u.PasswordResetToken == tokenHash, cancellationToken);
    }
}
