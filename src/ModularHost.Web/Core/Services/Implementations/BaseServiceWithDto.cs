using AutoMapper;
using ModularHost.Web.Core.Services.Interfaces;
using ModularHost.Web.Core.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ModularHost.Web.Core.Services
{
    public abstract class BaseServiceWithDto<TEntity, TDto, TCreateDto, TUpdateDto> : IBaseService<TEntity, TDto, TCreateDto, TUpdateDto>
        where TEntity : BaseEntity
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IRepository<TEntity> _repository;
        protected readonly IMapper _mapper;

        protected BaseServiceWithDto(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<TEntity>();
            _mapper = mapper;
        }
        
        protected BaseServiceWithDto(IRepository<TEntity> repository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public virtual async Task<TDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<TDto>(entity);
        }

        public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TDto>>(entities);
        }

        public virtual async Task<IEnumerable<TDto>> GetPagedAsync(int page, int pageSize, Expression<Func<TEntity, bool>>? predicate = null)
        {
            var query = _repository.QueryNoTracking();
            
            if (predicate != null)
                query = query.Where(predicate);

            var entities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TDto>>(entities);
        }

        public virtual async Task<IEnumerable<TDto>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = await _repository.FindAsync(predicate);
            return _mapper.Map<IEnumerable<TDto>>(entities);
        }

        public virtual async Task<TDto> CreateAsync(TCreateDto createDto)
        {
            var entity = _mapper.Map<TEntity>(createDto);
            
            // Set audit fields
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            
            await BeforeCreateAsync(entity, createDto);
            
            await _repository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            
            await AfterCreateAsync(entity, createDto);
            
            return _mapper.Map<TDto>(entity);
        }

        public virtual async Task<TDto?> UpdateAsync(Guid id, TUpdateDto updateDto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return null;

            _mapper.Map(updateDto, entity);
            
            // Set audit fields
            entity.UpdatedAt = DateTime.UtcNow;
            
            await BeforeUpdateAsync(entity, updateDto);
            
            _repository.Update(entity);
            await _unitOfWork.SaveChangesAsync();
            
            await AfterUpdateAsync(entity, updateDto);
            
            return _mapper.Map<TDto>(entity);
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return false;

            await BeforeDeleteAsync(entity);
            
            _repository.Remove(entity);
            await _unitOfWork.SaveChangesAsync();
            
            await AfterDeleteAsync(entity);
            
            return true;
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _repository.ExistsAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            return await _repository.CountAsync(predicate);
        }

        public virtual IQueryable<TEntity> GetQueryable()
        {
            return _repository.Query();
        }

        // Hook methods for derived classes to override
        protected virtual Task BeforeCreateAsync(TEntity entity, TCreateDto dto) => Task.CompletedTask;
        protected virtual Task AfterCreateAsync(TEntity entity, TCreateDto dto) => Task.CompletedTask;
        protected virtual Task BeforeUpdateAsync(TEntity entity, TUpdateDto dto) => Task.CompletedTask;
        protected virtual Task AfterUpdateAsync(TEntity entity, TUpdateDto dto) => Task.CompletedTask;
        protected virtual Task BeforeDeleteAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task AfterDeleteAsync(TEntity entity) => Task.CompletedTask;
    }
}