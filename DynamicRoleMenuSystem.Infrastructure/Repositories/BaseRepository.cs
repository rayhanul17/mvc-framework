using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    protected readonly IServiceProvider _serviceProvider;
    protected string? CurrentUserId => _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public BaseRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor = null!, IServiceProvider serviceProvider = null!)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
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
        
        // Log the creation if entity is BaseEntity and not a Log itself
        if (entity is BaseEntity baseEntity2 && !(entity is Log) && !(entity is LogArchive))
        {
            await LogEntityChangeAsync("Create", null, entity, baseEntity2.Id);
        }
        
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
        // Get original entity for logging
        T? originalEntity = null;
        if (entity is BaseEntity baseEntity && !(entity is Log) && !(entity is LogArchive))
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                originalEntity = _dbSet.Find(baseEntity.Id);
            }
            else
            {
                originalEntity = (T)entry.OriginalValues.ToObject();
            }
            
            baseEntity.UpdatedAt = DateTime.UtcNow;
            baseEntity.ModifiedBy = CurrentUserId;
        }
        
        _dbSet.Update(entity);
        
        // Log the update if entity is BaseEntity and not a Log itself
        if (entity is BaseEntity baseEntity2 && !(entity is Log) && !(entity is LogArchive) && originalEntity != null)
        {
            LogEntityChangeAsync("Update", originalEntity, entity, baseEntity2.Id).GetAwaiter().GetResult();
        }
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
        // Log the deletion if entity is BaseEntity and not a Log itself
        if (entity is BaseEntity baseEntity && !(entity is Log) && !(entity is LogArchive))
        {
            LogEntityChangeAsync("Delete", entity, null, baseEntity.Id).GetAwaiter().GetResult();
        }
        
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
    
    private async Task LogEntityChangeAsync(string action, T? oldEntity, T? newEntity, int entityId)
    {
        try
        {
            if (_serviceProvider == null) return;
            
            using var scope = _serviceProvider.CreateScope();
            var logRepository = scope.ServiceProvider.GetService<ILogRepository>();
            
            if (logRepository == null) return;
            
            var tableName = typeof(T).Name;
            string? oldValues = oldEntity != null ? JsonSerializer.Serialize(oldEntity) : null;
            string? newValues = newEntity != null ? JsonSerializer.Serialize(newEntity) : null;
            string? changes = GetChanges(oldEntity, newEntity);
            
            var httpContext = scope.ServiceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
            var log = new Log
            {
                TableName = tableName,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                Changes = changes,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                LoggedAt = DateTime.UtcNow,
                UserId = CurrentUserId
            };
            
            await logRepository.AddAsync(log);
            var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();
            if (unitOfWork != null)
            {
                await unitOfWork.SaveChangesAsync();
            }
        }
        catch
        {
            // Silently fail logging to not interrupt the main operation
        }
    }
    
    private string? GetChanges(T? oldEntity, T? newEntity)
    {
        if (oldEntity == null || newEntity == null) return null;
        
        var changes = new List<string>();
        var properties = typeof(T).GetProperties();
        
        foreach (var property in properties)
        {
            if (property.Name == "UpdatedAt" || property.Name == "ModifiedBy")
                continue;
                
            var oldValue = property.GetValue(oldEntity);
            var newValue = property.GetValue(newEntity);
            
            if (!Equals(oldValue, newValue))
            {
                // Convert values to string safely, handling nulls and complex types
                var oldValueStr = ConvertToString(oldValue);
                var newValueStr = ConvertToString(newValue);
                changes.Add($"{property.Name}: {oldValueStr} -> {newValueStr}");
            }
        }
        
        return changes.Any() ? string.Join(", ", changes) : null;
    }
    
    private string ConvertToString(object? value)
    {
        if (value == null)
            return "null";
            
        // Handle dates specifically
        if (value is DateTime dateTime)
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            
        // Handle enums
        if (value.GetType().IsEnum)
            return value.ToString() ?? "null";
            
        // Handle simple types
        if (value is string || value.GetType().IsPrimitive || value is decimal)
            return value.ToString() ?? "null";
            
        // For complex types, just use the type name to avoid serialization issues
        return $"[{value.GetType().Name}]";
    }
}