using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DynamicRoleMenuSystem.Core.Interfaces;
using DynamicRoleMenuSystem.Infrastructure.Data;
using DynamicRoleMenuSystem.Core.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DynamicRoleMenuSystem.Infrastructure.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected string? CurrentUserId => _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public BaseRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor = null!)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<T> AddAsync(T entity)
    {
        // Set CreatedBy and CreatedAt if entity inherits from BaseEntity
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.CreatedAt = DateTime.UtcNow;
            baseEntity.CreatedBy = CurrentUserId;
        }
        
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        var entityList = entities.ToList();
        
        // Set CreatedBy and CreatedAt for BaseEntity types
        foreach (var entity in entityList)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.CreatedAt = DateTime.UtcNow;
                baseEntity.CreatedBy = CurrentUserId;
            }
        }
        
        await _dbSet.AddRangeAsync(entityList);
    }

    public void Update(T entity)
    {
        // Set ModifiedBy and UpdatedAt if entity inherits from BaseEntity
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.UpdatedAt = DateTime.UtcNow;
            baseEntity.ModifiedBy = CurrentUserId;
        }
        
        _dbSet.Update(entity);
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
        var entityList = entities.ToList();
        
        // Set ModifiedBy and UpdatedAt for BaseEntity types
        foreach (var entity in entityList)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.UpdatedAt = DateTime.UtcNow;
                baseEntity.ModifiedBy = CurrentUserId;
            }
        }
        
        _dbSet.UpdateRange(entityList);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();
        
        return await _dbSet.CountAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }
}