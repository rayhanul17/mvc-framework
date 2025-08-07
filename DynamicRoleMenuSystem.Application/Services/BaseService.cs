using System.Linq.Expressions;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Core.Common;
using DynamicRoleMenuSystem.Core.Interfaces;

namespace DynamicRoleMenuSystem.Application.Services;

public class BaseService<T> : IBaseService<T> where T : class
{
    protected readonly IUnitOfWork _unitOfWork;

    public BaseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public virtual async Task<Result<T>> GetByIdAsync(object id)
    {
        try
        {
            var entity = await _unitOfWork.Repository<T>().GetByIdAsync(id);
            if (entity == null)
                return Result<T>.Failure($"Entity with id {id} not found");
            
            return Result<T>.Success(entity);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Error retrieving entity: {ex.Message}");
        }
    }

    public virtual async Task<Result<IEnumerable<T>>> GetAllAsync()
    {
        try
        {
            var entities = await _unitOfWork.Repository<T>().GetAllAsync();
            return Result<IEnumerable<T>>.Success(entities);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<T>>.Failure($"Error retrieving entities: {ex.Message}");
        }
    }

    public virtual async Task<Result<IEnumerable<T>>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var entities = await _unitOfWork.Repository<T>().FindAsync(predicate);
            return Result<IEnumerable<T>>.Success(entities);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<T>>.Failure($"Error finding entities: {ex.Message}");
        }
    }

    public virtual async Task<Result<T>> CreateAsync(T entity)
    {
        try
        {
            await _unitOfWork.Repository<T>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return Result<T>.Success(entity);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Error creating entity: {ex.Message}");
        }
    }

    public virtual async Task<Result<T>> UpdateAsync(T entity)
    {
        try
        {
            _unitOfWork.Repository<T>().Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return Result<T>.Success(entity);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Error updating entity: {ex.Message}");
        }
    }

    public virtual async Task<Result> DeleteAsync(object id)
    {
        try
        {
            var entity = await _unitOfWork.Repository<T>().GetByIdAsync(id);
            if (entity == null)
                return Result.Failure($"Entity with id {id} not found");
            
            _unitOfWork.Repository<T>().Remove(entity);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting entity: {ex.Message}");
        }
    }

    public virtual async Task<Result<int>> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        try
        {
            var count = await _unitOfWork.Repository<T>().CountAsync(predicate);
            return Result<int>.Success(count);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error counting entities: {ex.Message}");
        }
    }
}