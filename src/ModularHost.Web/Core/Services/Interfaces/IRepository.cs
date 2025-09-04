using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MRCMS.Core.Services.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Existing methods
        Task<T> GetByIdAsync(Guid id);
        Task<T> GetByIdAsync(int id);
        Task<T> GetByIdAsync(long id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        IQueryable<T> Query();
        IQueryable<T> QueryNoTracking();
        
        // Raw SQL Query methods
        Task<IEnumerable<T>> FromSqlRawAsync(string sql, params object[] parameters);
        Task<IEnumerable<TResult>> QuerySqlAsync<TResult>(string sql, params object[] parameters) where TResult : class;
        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
        Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql);
        
        // Transaction methods
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel);
        Task CommitTransactionAsync(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction);
        Task RollbackTransactionAsync(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction);
        
        // Stored Procedure methods
        Task<IEnumerable<TResult>> ExecuteStoredProcedureAsync<TResult>(string procedureName, params object[] parameters) where TResult : class;
        Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, params object[] parameters);
        
        // Bulk operations
        Task BulkInsertAsync(IEnumerable<T> entities);
        Task BulkUpdateAsync(IEnumerable<T> entities);
        Task BulkDeleteAsync(IEnumerable<T> entities);
        Task BulkMergeAsync(IEnumerable<T> entities);
        
        // Advanced query methods with transaction support
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        Task ExecuteInTransactionAsync(Func<Task> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        
        // DataTable support for complex queries
        Task<DataTable> GetDataTableAsync(string sql, params object[] parameters);
        Task<DataSet> GetDataSetAsync(string sql, params object[] parameters);
    }
    
}