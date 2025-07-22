using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using mvc.framework.Models;

namespace mvc.framework.Data
{
    public class BaseRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public IQueryable<T> GetAll() => _context.Set<T>();
        public async Task<T?> GetByIdAsync(Guid id) => await _context.Set<T>().FindAsync(id);
        public async Task AddAsync(T entity, string user)
        {
            entity.CreatedDate = DateTime.UtcNow;
            entity.CreatedBy = user;
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(T entity, string user)
        {
            entity.ModifiedDate = DateTime.UtcNow;
            entity.ModifiedBy = user;
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
