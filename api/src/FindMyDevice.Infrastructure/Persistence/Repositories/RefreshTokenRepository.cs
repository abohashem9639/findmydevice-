using Microsoft.EntityFrameworkCore;
using FindMyDevice.Application.Interfaces;
using FindMyDevice.Domain.Entities;
using FindMyDevice.Infrastructure.Persistence.Contexts;

namespace FindMyDevice.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly FindMyDeviceDbContext _dbContext;

        public RefreshTokenRepository(FindMyDeviceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, bool includeUser, CancellationToken cancellationToken)
        {
            var query = _dbContext.RefreshTokens.AsQueryable();

            if (includeUser)
            {
                query = query.Include(x => x.User);
            }

            return await query.SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        }

        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        }

        public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            _dbContext.RefreshTokens.Update(refreshToken);
            return Task.CompletedTask;
        }
    }
}
