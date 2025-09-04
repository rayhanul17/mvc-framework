using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MRCMS.Core.Models
{
    /// <summary>
    /// DataTables request model for server-side processing
    /// </summary>
    public class DataTableRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public DataTableSearch Search { get; set; }
        public List<DataTableOrder> Order { get; set; }
        public List<DataTableColumn> Columns { get; set; }

        public DataTableRequest()
        {
            Search = new DataTableSearch();
            Order = new List<DataTableOrder>();
            Columns = new List<DataTableColumn>();
        }

        // Helper properties
        public int PageNumber => (Start / Length) + 1;
        public string OrderBy => GetOrderByString();
        public bool IsAscending => Order.FirstOrDefault()?.Dir == "asc";

        private string GetOrderByString()
        {
            if (Order == null || !Order.Any())
                return string.Empty;

            var orderColumn = Order.First();
            if (Columns != null && orderColumn.Column < Columns.Count)
            {
                return Columns[orderColumn.Column].Data;
            }

            return string.Empty;
        }
    }

    public class DataTableSearch
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class DataTableOrder
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }

    public class DataTableColumn
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public DataTableSearch Search { get; set; }

        public DataTableColumn()
        {
            Search = new DataTableSearch();
        }
    }

    /// <summary>
    /// DataTables response model for server-side processing
    /// </summary>
    public class DataTableResponse<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public IEnumerable<T> Data { get; set; }
        public string Error { get; set; }

        public DataTableResponse()
        {
            Data = new List<T>();
        }

        public static DataTableResponse<T> Create(
            DataTableRequest request,
            IEnumerable<T> data,
            int totalRecords,
            int filteredRecords)
        {
            return new DataTableResponse<T>
            {
                Draw = request.Draw,
                Data = data,
                RecordsTotal = totalRecords,
                RecordsFiltered = filteredRecords
            };
        }

        public static DataTableResponse<T> CreateError(DataTableRequest request, string error)
        {
            return new DataTableResponse<T>
            {
                Draw = request.Draw,
                Error = error,
                Data = new List<T>()
            };
        }
    }

    /// <summary>
    /// Configuration for DataTable columns
    /// </summary>
    public class DataTableColumnDefinition
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public bool Searchable { get; set; } = true;
        public bool Orderable { get; set; } = true;
        public bool Visible { get; set; } = true;
        public string Width { get; set; }
        public string ClassName { get; set; }
        public string DefaultContent { get; set; }
        public string Render { get; set; }
        public string Type { get; set; }

        public static DataTableColumnDefinition Create(
            string data,
            string title,
            bool searchable = true,
            bool orderable = true,
            string width = null,
            string className = null)
        {
            return new DataTableColumnDefinition
            {
                Data = data,
                Name = data,
                Title = title,
                Searchable = searchable,
                Orderable = orderable,
                Width = width,
                ClassName = className
            };
        }
    }

    /// <summary>
    /// DataTable configuration model
    /// </summary>
    public class DataTableConfiguration
    {
        public string TableId { get; set; }
        public string AjaxUrl { get; set; }
        public List<DataTableColumnDefinition> Columns { get; set; }
        public bool ServerSide { get; set; } = true;
        public bool Processing { get; set; } = true;
        public bool Responsive { get; set; } = true;
        public bool AutoWidth { get; set; } = false;
        public int PageLength { get; set; } = 10;
        public List<int> LengthMenu { get; set; }
        public string Dom { get; set; } = "Bfrtip";
        public List<string> Buttons { get; set; }
        public string Language { get; set; }
        public Dictionary<string, object> AdditionalOptions { get; set; }

        public DataTableConfiguration()
        {
            Columns = new List<DataTableColumnDefinition>();
            LengthMenu = new List<int> { 10, 25, 50, 100 };
            Buttons = new List<string> { "copy", "csv", "excel", "pdf", "print", "colvis" };
            AdditionalOptions = new Dictionary<string, object>();
        }

        public static DataTableConfiguration Default(string tableId, string ajaxUrl)
        {
            return new DataTableConfiguration
            {
                TableId = tableId,
                AjaxUrl = ajaxUrl,
                ServerSide = true,
                Processing = true,
                Responsive = true,
                AutoWidth = false,
                PageLength = 10
            };
        }
    }

    /// <summary>
    /// Extension methods for DataTable processing
    /// </summary>
    public static class DataTableExtensions
    {
        public static IQueryable<T> ApplyDataTableFilters<T>(
            this IQueryable<T> query,
            DataTableRequest request,
            Func<T, string, bool> searchPredicate = null)
        {
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search?.Value) && searchPredicate != null)
            {
                query = query.Where(x => searchPredicate(x, request.Search.Value));
            }

            return query;
        }

        public static IQueryable<T> ApplyDataTableOrdering<T>(
            this IQueryable<T> query,
            DataTableRequest request)
        {
            if (request.Order == null || !request.Order.Any())
                return query;

            var orderColumn = request.Order.First();
            if (request.Columns == null || orderColumn.Column >= request.Columns.Count)
                return query;

            var columnName = request.Columns[orderColumn.Column].Data;
            if (string.IsNullOrWhiteSpace(columnName))
                return query;

            // Use dynamic LINQ for ordering
            // Note: In production, you might want to use a library like System.Linq.Dynamic.Core
            // For now, we'll use reflection
            var propertyInfo = typeof(T).GetProperty(columnName);
            if (propertyInfo == null)
                return query;

            if (orderColumn.Dir == "desc")
            {
                return query.OrderByDescending(x => propertyInfo.GetValue(x));
            }
            else
            {
                return query.OrderBy(x => propertyInfo.GetValue(x));
            }
        }

        public static IQueryable<T> ApplyDataTablePaging<T>(
            this IQueryable<T> query,
            DataTableRequest request)
        {
            return query
                .Skip(request.Start)
                .Take(request.Length);
        }

        public static async Task<DataTableResponse<T>> ToDataTableResponseAsync<T>(
            this IQueryable<T> query,
            DataTableRequest request,
            Func<T, string, bool> searchPredicate = null)
        {
            // Get total count before filtering
            var totalRecords = await query.CountAsync();

            // Apply filters
            var filteredQuery = query.ApplyDataTableFilters(request, searchPredicate);

            // Get filtered count
            var filteredRecords = await filteredQuery.CountAsync();

            // Apply ordering and paging
            var data = await filteredQuery
                .ApplyDataTableOrdering(request)
                .ApplyDataTablePaging(request)
                .ToListAsync();

            return DataTableResponse<T>.Create(request, data, totalRecords, filteredRecords);
        }
    }

    /// <summary>
    /// DataTable AJAX result helpers
    /// </summary>
    public static class DataTableJsonHelper
    {
        public static object FormatForDataTable<T>(
            this IEnumerable<T> data,
            int draw,
            int recordsTotal,
            int recordsFiltered)
        {
            return new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            };
        }

        public static object ErrorForDataTable(int draw, string error)
        {
            return new
            {
                draw = draw,
                error = error,
                data = new object[0]
            };
        }
    }
}