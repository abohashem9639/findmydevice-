using FindMyDevice.Application.Interfaces;
using FindMyDevice.Infrastructure.Persistence.Contexts;

namespace FindMyDevice.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FindMyDeviceDbContext _dbContext;

        public UnitOfWork(FindMyDeviceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);
    }
}
