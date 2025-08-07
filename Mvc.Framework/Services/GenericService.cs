using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using mvc.framework.Data;
using mvc.framework.Models;

namespace mvc.framework.Services
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        protected readonly IGenericRepository<T> _repository;
        protected readonly ILogger<GenericService<T>> _logger;

        public GenericService(IGenericRepository<T> repository, ILogger<GenericService<T>> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public virtual async Task<T> GetByIdAsync(object id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting {typeof(T).Name} by id: {id}");
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting all {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _repository.FindAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            try
            {
                if (entity is BaseEntity baseEntity)
                {
                    baseEntity.CreatedDate = DateTime.UtcNow;
                    baseEntity.ModifiedDate = DateTime.UtcNow;
                }

                await _repository.AddAsync(entity);
                await _repository.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                var entityList = entities.ToList();
                foreach (var entity in entityList)
                {
                    if (entity is BaseEntity baseEntity)
                    {
                        baseEntity.CreatedDate = DateTime.UtcNow;
                        baseEntity.ModifiedDate = DateTime.UtcNow;
                    }
                }

                await _repository.AddRangeAsync(entityList);
                await _repository.SaveChangesAsync();
                return entityList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating range of {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                if (entity is BaseEntity baseEntity)
                {
                    baseEntity.ModifiedDate = DateTime.UtcNow;
                }

                _repository.Update(entity);
                await _repository.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                var entityList = entities.ToList();
                foreach (var entity in entityList)
                {
                    if (entity is BaseEntity baseEntity)
                    {
                        baseEntity.ModifiedDate = DateTime.UtcNow;
                    }
                }

                _repository.UpdateRange(entityList);
                await _repository.SaveChangesAsync();
                return entityList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating range of {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(object id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    return false;
                }

                _repository.Remove(entity);
                await _repository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {typeof(T).Name} by id: {id}");
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(T entity)
        {
            try
            {
                _repository.Remove(entity);
                await _repository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<bool> DeleteRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                _repository.RemoveRange(entities);
                await _repository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting range of {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<int> CountAsync()
        {
            try
            {
                return await _repository.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _repository.CountAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting {typeof(T).Name} with predicate");
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _repository.AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking existence of {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>> filter = null)
        {
            try
            {
                var query = _repository.Query();

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                var totalCount = await query.CountAsync();
                
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting paged {typeof(T).Name}");
                throw;
            }
        }
    }
}