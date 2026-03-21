namespace e360_clone_fe.Models.ViewModels
{
    /// <summary>
    /// Class ViewModel
    /// </summary>
    public class ClassViewModel
    {
        public int Id { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public int MajorId { get; set; }
        public string Status { get; set; } = "Active";

        public string MajorName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Paged ViewModel for pagination support
    /// </summary>
    public class PagedViewModel<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }
        public int TotalPages => TotalRecords > 0 && PageSize > 0
            ? (int)Math.Ceiling(TotalRecords / (double)PageSize)
            : 0;
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

        // Search and filter
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public Dictionary<string, string> Filters { get; set; } = new();

        // Pagination helpers
        public int StartRecord => (PageNumber - 1) * PageSize + 1;
        public int EndRecord => Math.Min(PageNumber * PageSize, TotalRecords);

        public static PagedViewModel<T> Create(
            IEnumerable<T> items,
            int pageNumber,
            int pageSize,
            int totalRecords)
        {
            return new PagedViewModel<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }
    }

    /// <summary>
    /// Request model for paged requests
    /// </summary>
    public class PagedRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public Dictionary<string, string> Filters { get; set; } = new();

        public Dictionary<string, string> ToQueryParams()
        {
            var queryParams = new Dictionary<string, string>
            {
                { "pageNumber", PageNumber.ToString() },
                { "pageSize", PageSize.ToString() }
            };

            if (!string.IsNullOrEmpty(SearchTerm))
                queryParams["searchTerm"] = SearchTerm;

            if (!string.IsNullOrEmpty(SortBy))
            {
                queryParams["sortBy"] = SortBy;
                queryParams["sortDescending"] = SortDescending.ToString().ToLower();
            }

            foreach (var filter in Filters)
            {
                queryParams[filter.Key] = filter.Value;
            }

            return queryParams;
        }
    }
}
