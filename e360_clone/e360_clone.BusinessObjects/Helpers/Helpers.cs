namespace e360_clone.BusinessObjects.Helpers
{
    /// <summary>
    /// Helper class for common queries
    /// </summary>
    public static class QueryHelpers
    {
        /// <summary>
        /// Tạo filter cho ngày bắt đầu và kết thúc
        /// </summary>
        public static Func<T, bool> DateRangeFilter<T>(
            Func<T, DateTime> dateProperty,
            DateTime? startDate,
            DateTime? endDate)
        {
            return entity =>
            {
                var date = dateProperty(entity);
                var afterStart = !startDate.HasValue || date >= startDate.Value;
                var beforeEnd = !endDate.HasValue || date <= endDate.Value;
                return afterStart && beforeEnd;
            };
        }

        /// <summary>
        /// Tạo filter cho trạng thái active
        /// </summary>
        public static Func<T, bool> ActiveFilter<T>(Func<T, string> statusProperty)
        {
            return entity => statusProperty(entity) == "Active";
        }

        /// <summary>
        /// Tạo filter tìm kiếm theo nhiều trường
        /// </summary>
        public static Func<T, bool> MultiFieldSearchFilter<T>(
            string searchTerm,
            params Func<T, string>[] propertySelectors)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return _ => true;
            }

            searchTerm = searchTerm.ToLower();
            return entity => propertySelectors.Any(selector =>
                selector(entity)?.ToLower().Contains(searchTerm) == true);
        }
    }

    /// <summary>
    /// Response wrapper cho API
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ApiResponse<T> SuccessResponse(T data, string message = "Thành công")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    /// <summary>
    /// Paged response cho API
    /// </summary>
    public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);

        public static PagedApiResponse<T> Create(
            IEnumerable<T> items,
            int pageNumber,
            int pageSize,
            int totalRecords,
            string message = "Thành công")
        {
            return new PagedApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }
    }

    /// <summary>
    /// Pagination metadata
    /// </summary>
    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public static PaginationMetadata Create(int currentPage, int pageSize, int totalItems)
        {
            return new PaginationMetadata
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }
    }
}
