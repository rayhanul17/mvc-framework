using DynamicRoleMenuSystem.Core.Common;
using System.Linq.Expressions;

namespace DynamicRoleMenuSystem.Application.Interfaces;

public interface IBaseService<T> where T : class
{
    Task<Result<T>> GetByIdAsync(object id);
    Task<Result<IEnumerable<T>>> GetAllAsync();
    Task<Result<IEnumerable<T>>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<Result<T>> CreateAsync(T entity);
    Task<Result<T>> UpdateAsync(T entity);
    Task<Result> DeleteAsync(object id);
    Task<Result<int>> CountAsync(Expression<Func<T, bool>>? predicate = null);
}