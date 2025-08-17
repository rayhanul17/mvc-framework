using Nexora.Core.Common;
using System.Data;
using System.Linq.Expressions;

namespace Nexora.Application.Interfaces;

public interface IBaseService<T> where T : class
{
    Task<Result<T>> GetByIdAsync(object id);
    Task<Result<IEnumerable<T>>> GetAllAsync();
    Task<Result<IEnumerable<T>>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<Result<T>> CreateAsync(T entity);
    Task<Result<T>> UpdateAsync(T entity);
    Task<Result> DeleteAsync(object id);
    Task<Result<int>> CountAsync(Expression<Func<T, bool>>? predicate = null);
    
    Task<Result<int>> ExecuteRawSqlAsync(string sql, params object[] parameters);
    Task<Result<DataTable>> LoadDataTableAsync(string sql, params object[] parameters);
}