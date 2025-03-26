namespace vnvt_back_end.Application.Models
{
    public class PagingParameters
    {
        private const int maxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = (value > maxPageSize) ? maxPageSize : value; }
        }

        private string? _keyword;
        public string? Keyword
        {
            get { return _keyword; }
            set { _keyword = string.IsNullOrWhiteSpace(value) ? null : value; }
        }

        private string? _sortField;
        public string? SortField
        {
            get { return _sortField; }
            set { _sortField = string.IsNullOrWhiteSpace(value) ? null : value; }
        }

        public bool SortDescending { get; set; } = false;
    }
}
