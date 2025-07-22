using System;
using System.Linq;
using System.Threading.Tasks;
using mvc.framework.Data;
using mvc.framework.Models;

namespace mvc.framework.Services
{
    public class BaseService<T> where T : BaseEntity
    {
        protected readonly BaseRepository<T> _repository;
        public BaseService(BaseRepository<T> repository)
        {
            _repository = repository;
        }
        public IQueryable<T> GetAll() => _repository.GetAll();
        public Task<T?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);
        public Task AddAsync(T entity, string user) => _repository.AddAsync(entity, user);
        public Task UpdateAsync(T entity, string user) => _repository.UpdateAsync(entity, user);
        public Task DeleteAsync(Guid id) => _repository.DeleteAsync(id);
    }
}
