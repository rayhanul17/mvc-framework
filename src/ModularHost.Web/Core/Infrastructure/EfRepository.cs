using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MySqlConnector;

namespace MRCMS.Core.Infrastructure
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public EfRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        #region Existing Methods

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> GetByIdAsync(long id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate);
        }

        public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public virtual void Remove(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null 
                ? await _dbSet.CountAsync() 
                : await _dbSet.CountAsync(predicate);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet;
        }

        public virtual IQueryable<T> QueryNoTracking()
        {
            return _dbSet.AsNoTracking();
        }

        #endregion

        #region Raw SQL Query Methods

        public virtual async Task<IEnumerable<T>> FromSqlRawAsync(string sql, params object[] parameters)
        {
            return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();
        }

        public virtual async Task<IEnumerable<TResult>> QuerySqlAsync<TResult>(string sql, params object[] parameters) where TResult : class
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }
            }

            if (command.Connection.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync();
            }

            var results = new List<TResult>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    // Use reflection or manual mapping here
                    // For simplicity, assuming TResult has a parameterless constructor
                    var item = Activator.CreateInstance<TResult>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var property = typeof(TResult).GetProperty(reader.GetName(i));
                        if (property != null && !reader.IsDBNull(i))
                        {
                            property.SetValue(item, reader.GetValue(i));
                        }
                    }
                    results.Add(item);
                }
            }

            return results;
        }

        public virtual async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public virtual async Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql)
        {
            return await _context.Database.ExecuteSqlInterpolatedAsync(sql);
        }

        #endregion

        #region Transaction Methods

        public virtual async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public virtual async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return await _context.Database.BeginTransactionAsync(isolationLevel);
        }

        public virtual async Task CommitTransactionAsync(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
        {
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }
        }

        public virtual async Task RollbackTransactionAsync(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
        }

        public virtual async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<Task<TResult>> operation, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            using var transaction = await BeginTransactionAsync(isolationLevel);
            try
            {
                var result = await operation();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public virtual async Task ExecuteInTransactionAsync(
            Func<Task> operation, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            using var transaction = await BeginTransactionAsync(isolationLevel);
            try
            {
                await operation();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region Stored Procedure Methods

        public virtual async Task<IEnumerable<TResult>> ExecuteStoredProcedureAsync<TResult>(
            string procedureName, 
            params object[] parameters) where TResult : class
        {
            var parameterString = string.Join(", ", 
                Enumerable.Range(0, parameters.Length).Select(i => $"@p{i}"));
            
            var sql = $"EXEC {procedureName} {parameterString}";
            
            return await QuerySqlAsync<TResult>(sql, parameters);
        }

        public virtual async Task<int> ExecuteStoredProcedureNonQueryAsync(
            string procedureName, 
            params object[] parameters)
        {
            var parameterString = string.Join(", ", 
                Enumerable.Range(0, parameters.Length).Select(i => $"@p{i}"));
            
            var sql = $"EXEC {procedureName} {parameterString}";
            
            return await ExecuteSqlRawAsync(sql, parameters);
        }

        #endregion

        #region Bulk Operations

        public virtual async Task BulkInsertAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task BulkUpdateAsync(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task BulkDeleteAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task BulkMergeAsync(IEnumerable<T> entities)
        {
            // This is a simplified version. For production use,
            // consider using a library like EFCore.BulkExtensions
            foreach (var entity in entities)
            {
                var entry = _context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }

                // Check if entity exists
                var keyValues = entry.Properties
                    .Where(p => p.Metadata.IsPrimaryKey())
                    .Select(p => p.CurrentValue)
                    .ToArray();

                var existingEntity = await _dbSet.FindAsync(keyValues);
                
                if (existingEntity != null)
                {
                    entry.State = EntityState.Modified;
                }
                else
                {
                    entry.State = EntityState.Added;
                }
            }
            
            await _context.SaveChangesAsync();
        }

        #endregion

        #region DataTable Support

        public virtual async Task<DataTable> GetDataTableAsync(string sql, params object[] parameters)
        {
            var dataTable = new DataTable();
            
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }
            }

            if (command.Connection.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync();
            }

            using var reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);
            
            return dataTable;
        }

        public virtual async Task<DataSet> GetDataSetAsync(string sql, params object[] parameters)
        {
            var dataSet = new DataSet();
            
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }
            }

            if (command.Connection.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync();
            }

            using var reader = await command.ExecuteReaderAsync();
            
            do
            {
                var dataTable = new DataTable();
                dataTable.Load(reader);
                dataSet.Tables.Add(dataTable);
            }
            while (!reader.IsClosed && reader.HasRows);
            
            return dataSet;
        }

        #endregion
    }
}