using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Interfaces;

namespace Nexora.Application.Services;

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

    public virtual async Task<Result<int>> ExecuteRawSqlAsync(string sql, params object[] parameters)
    {
        try
        {
            var context = _unitOfWork.GetDbContext();
            var result = await context.Database.ExecuteSqlRawAsync(sql, parameters);
            return Result<int>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error executing raw SQL: {ex.Message}");
        }
    }

    public virtual async Task<Result<DataTable>> LoadDataTableAsync(string sql, params object[] parameters)
    {
        try
        {
            var context = _unitOfWork.GetDbContext();
            var connectionString = context.Database.GetConnectionString();
            
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new MySqlCommand(sql, connection);
            
            for (int i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue($"@p{i}", parameters[i] ?? DBNull.Value);
            }
            
            using var adapter = new MySqlDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            
            return Result<DataTable>.Success(dataTable);
        }
        catch (Exception ex)
        {
            return Result<DataTable>.Failure($"Error loading data: {ex.Message}");
        }
    }
}