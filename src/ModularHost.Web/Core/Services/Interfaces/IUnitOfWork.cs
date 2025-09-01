using System;
using System.Threading.Tasks;

namespace MRCMS.Core.Services.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
        int SaveChanges();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}