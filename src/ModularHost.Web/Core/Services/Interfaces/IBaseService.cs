using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MRCMS.Core.Services.Interfaces
{
    public interface IBaseService<TEntity, TDto, TCreateDto, TUpdateDto> 
        where TEntity : class
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class
    {
        Task<TDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<IEnumerable<TDto>> GetPagedAsync(int page, int pageSize, Expression<Func<TEntity, bool>>? predicate = null);
        Task<IEnumerable<TDto>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TDto> CreateAsync(TCreateDto createDto);
        Task<TDto?> UpdateAsync(Guid id, TUpdateDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);
        IQueryable<TEntity> GetQueryable();
    }
}