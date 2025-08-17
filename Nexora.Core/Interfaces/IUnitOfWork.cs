using Microsoft.EntityFrameworkCore;

namespace Nexora.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IBaseRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    DbContext GetDbContext();
}