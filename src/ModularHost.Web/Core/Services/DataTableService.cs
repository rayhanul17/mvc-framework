using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Infrastructure;
using MRCMS.Core.Models;
using MRCMS.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace MRCMS.Core.Services
{
    public interface IDataTableService
    {
        Task<DataTableResponse<T>> GetDataTableResponseAsync<T>(
            DataTableRequest request,
            IQueryable<T> query,
            Expression<Func<T, bool>> searchExpression = null) where T : class;

        Task<DataTableResponse<T>> GetDataTableResponseWithRawSqlAsync<T>(
            DataTableRequest request,
            string sql,
            params object[] parameters) where T : class, new();

        Task<DataTable> GetRawDataTableAsync(
            string sql,
            params object[] parameters);

        IQueryable<T> ApplyDataTableFilters<T>(
            IQueryable<T> query,
            DataTableRequest request,
            List<string> searchableColumns) where T : class;

        IQueryable<T> ApplyDataTableSorting<T>(
            IQueryable<T> query,
            DataTableRequest request) where T : class;

        IQueryable<T> ApplyDataTablePaging<T>(
            IQueryable<T> query,
            DataTableRequest request) where T : class;
    }

    public class DataTableService : IDataTableService
    {
        private readonly AppDbContext _context;

        public DataTableService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DataTableResponse<T>> GetDataTableResponseAsync<T>(
            DataTableRequest request,
            IQueryable<T> query,
            Expression<Func<T, bool>> searchExpression = null) where T : class
        {
            try
            {
                // Get total count before filtering
                var totalRecords = await query.CountAsync();

                // Apply global search filter if provided
                if (!string.IsNullOrWhiteSpace(request.Search?.Value) && searchExpression != null)
                {
                    query = query.Where(searchExpression);
                }

                // Apply column-specific filters
                query = ApplyColumnFilters(query, request);

                // Get filtered count
                var filteredRecords = await query.CountAsync();

                // Apply sorting
                query = ApplyDataTableSorting(query, request);

                // Apply paging
                query = ApplyDataTablePaging(query, request);

                // Get the data
                var data = await query.ToListAsync();

                return DataTableResponse<T>.Create(request, data, totalRecords, filteredRecords);
            }
            catch (Exception ex)
            {
                return DataTableResponse<T>.CreateError(request, ex.Message);
            }
        }

        public async Task<DataTableResponse<T>> GetDataTableResponseWithRawSqlAsync<T>(
            DataTableRequest request,
            string sql,
            params object[] parameters) where T : class, new()
        {
            try
            {
                // Build counting query
                var countSql = $"SELECT COUNT(*) FROM ({sql}) AS CountQuery";
                
                using var countCommand = _context.Database.GetDbConnection().CreateCommand();
                countCommand.CommandText = countSql;
                countCommand.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        countCommand.Parameters.Add(param);
                    }
                }

                if (countCommand.Connection.State != ConnectionState.Open)
                {
                    await countCommand.Connection.OpenAsync();
                }

                var totalRecords = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                var filteredRecords = totalRecords;

                // Apply search filter to SQL if needed
                var finalSql = sql;
                if (!string.IsNullOrWhiteSpace(request.Search?.Value))
                {
                    // This is a simplified approach - you might need to customize based on your needs
                    finalSql = $@"SELECT * FROM ({sql}) AS SearchQuery 
                                 WHERE CONCAT_WS(' ', {GetSearchableColumns<T>()}) 
                                 LIKE @searchTerm";
                    
                    // Add search parameter
                    var searchParams = parameters.ToList();
                    searchParams.Add(new MySqlConnector.MySqlParameter("@searchTerm", $"%{request.Search.Value}%"));
                    parameters = searchParams.ToArray();

                    // Get filtered count
                    var filteredCountSql = $"SELECT COUNT(*) FROM ({finalSql}) AS FilteredCountQuery";
                    using var filteredCountCommand = _context.Database.GetDbConnection().CreateCommand();
                    filteredCountCommand.CommandText = filteredCountSql;
                    filteredCountCommand.CommandType = CommandType.Text;

                    foreach (var param in parameters)
                    {
                        filteredCountCommand.Parameters.Add(param);
                    }

                    filteredRecords = Convert.ToInt32(await filteredCountCommand.ExecuteScalarAsync());
                }

                // Apply ordering and paging
                finalSql = ApplySqlOrdering(finalSql, request);
                finalSql = ApplySqlPaging(finalSql, request);

                // Execute the final query
                var data = await ExecuteSqlQuery<T>(finalSql, parameters);

                return DataTableResponse<T>.Create(request, data, totalRecords, filteredRecords);
            }
            catch (Exception ex)
            {
                return DataTableResponse<T>.CreateError(request, ex.Message);
            }
        }

        public async Task<DataTable> GetRawDataTableAsync(string sql, params object[] parameters)
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

        public IQueryable<T> ApplyDataTableFilters<T>(
            IQueryable<T> query,
            DataTableRequest request,
            List<string> searchableColumns) where T : class
        {
            if (string.IsNullOrWhiteSpace(request.Search?.Value))
                return query;

            var searchValue = request.Search.Value.ToLower();
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression searchExpression = null;

            foreach (var column in searchableColumns)
            {
                var property = Expression.Property(parameter, column);
                var propertyAsString = Expression.Call(property, "ToString", null);
                var toLower = Expression.Call(propertyAsString, "ToLower", null);
                var contains = Expression.Call(toLower, "Contains", null, Expression.Constant(searchValue));

                searchExpression = searchExpression == null
                    ? contains
                    : Expression.OrElse(searchExpression, contains);
            }

            if (searchExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        public IQueryable<T> ApplyDataTableSorting<T>(
            IQueryable<T> query,
            DataTableRequest request) where T : class
        {
            if (request.Order == null || !request.Order.Any())
                return query;

            var orderColumn = request.Order.First();
            if (request.Columns == null || orderColumn.Column >= request.Columns.Count)
                return query;

            var columnName = request.Columns[orderColumn.Column].Data;
            if (string.IsNullOrWhiteSpace(columnName))
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, columnName);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = orderColumn.Dir == "desc" ? "OrderByDescending" : "OrderBy";
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.Type },
                query.Expression,
                Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(resultExpression);
        }

        public IQueryable<T> ApplyDataTablePaging<T>(
            IQueryable<T> query,
            DataTableRequest request) where T : class
        {
            return query
                .Skip(request.Start)
                .Take(request.Length);
        }

        private IQueryable<T> ApplyColumnFilters<T>(IQueryable<T> query, DataTableRequest request) where T : class
        {
            if (request.Columns == null)
                return query;

            foreach (var column in request.Columns.Where(c => !string.IsNullOrWhiteSpace(c.Search?.Value)))
            {
                var searchValue = column.Search.Value.ToLower();
                var parameter = Expression.Parameter(typeof(T), "x");
                
                try
                {
                    var property = Expression.Property(parameter, column.Data);
                    Expression searchExpression;

                    if (property.Type == typeof(string))
                    {
                        var toLower = Expression.Call(property, "ToLower", null);
                        searchExpression = Expression.Call(toLower, "Contains", null, Expression.Constant(searchValue));
                    }
                    else if (property.Type == typeof(int) || property.Type == typeof(long) || 
                             property.Type == typeof(decimal) || property.Type == typeof(double))
                    {
                        if (double.TryParse(searchValue, out var numericValue))
                        {
                            var convertedValue = Convert.ChangeType(numericValue, property.Type);
                            searchExpression = Expression.Equal(property, Expression.Constant(convertedValue));
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (property.Type == typeof(bool))
                    {
                        if (bool.TryParse(searchValue, out var boolValue))
                        {
                            searchExpression = Expression.Equal(property, Expression.Constant(boolValue));
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (property.Type == typeof(DateTime) || property.Type == typeof(DateTime?))
                    {
                        if (DateTime.TryParse(searchValue, out var dateValue))
                        {
                            var startDate = dateValue.Date;
                            var endDate = startDate.AddDays(1);
                            
                            var greaterThanOrEqual = Expression.GreaterThanOrEqual(property, Expression.Constant(startDate));
                            var lessThan = Expression.LessThan(property, Expression.Constant(endDate));
                            searchExpression = Expression.AndAlso(greaterThanOrEqual, lessThan);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
                    query = query.Where(lambda);
                }
                catch
                {
                    // Property doesn't exist or other error - skip this filter
                    continue;
                }
            }

            return query;
        }

        private string GetSearchableColumns<T>() where T : class
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string))
                .Select(p => p.Name);

            return string.Join(", ", properties);
        }

        private string ApplySqlOrdering(string sql, DataTableRequest request)
        {
            if (request.Order == null || !request.Order.Any())
                return sql;

            var orderColumn = request.Order.First();
            if (request.Columns == null || orderColumn.Column >= request.Columns.Count)
                return sql;

            var columnName = request.Columns[orderColumn.Column].Data;
            if (string.IsNullOrWhiteSpace(columnName))
                return sql;

            var direction = orderColumn.Dir == "desc" ? "DESC" : "ASC";
            return $@"SELECT * FROM ({sql}) AS OrderedQuery 
                     ORDER BY {columnName} {direction}";
        }

        private string ApplySqlPaging(string sql, DataTableRequest request)
        {
            return $@"SELECT * FROM ({sql}) AS PagedQuery 
                     LIMIT {request.Length} OFFSET {request.Start}";
        }

        private async Task<IEnumerable<T>> ExecuteSqlQuery<T>(string sql, params object[] parameters) where T : class, new()
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

            var results = new List<T>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                var properties = typeof(T).GetProperties();
                
                while (await reader.ReadAsync())
                {
                    var item = new T();
                    
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var fieldName = reader.GetName(i);
                        var property = properties.FirstOrDefault(p => 
                            string.Equals(p.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                        
                        if (property != null && !reader.IsDBNull(i))
                        {
                            var value = reader.GetValue(i);
                            if (value != null)
                            {
                                try
                                {
                                    property.SetValue(item, Convert.ChangeType(value, property.PropertyType));
                                }
                                catch
                                {
                                    // Type conversion failed - skip this property
                                }
                            }
                        }
                    }
                    
                    results.Add(item);
                }
            }

            return results;
        }
    }
}