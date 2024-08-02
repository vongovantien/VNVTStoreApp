namespace vnvt_back_end.Application.Models
{
    public class PagingParameters
    {
        private const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = (value > maxPageSize) ? maxPageSize : value; }
        }

        public string Keyword { get; set; }
        public string SortField { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}