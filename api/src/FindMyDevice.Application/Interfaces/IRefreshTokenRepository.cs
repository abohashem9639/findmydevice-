using FindMyDevice.Domain.Entities;

namespace FindMyDevice.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, bool includeUser, CancellationToken cancellationToken);

        Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

        Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    }
}
