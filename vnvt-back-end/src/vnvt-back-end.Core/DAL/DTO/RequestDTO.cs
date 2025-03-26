using System.Collections.Generic;

namespace FW.WAPI.Core.DAL.DTO
{
    public class RequestDTO
    {
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public dynamic PostObject { get; set; }
        public List<string> Fields { get; set; }
        public SortDTO SortDTO { get; set; }
        public List<SearchDTO> Searching { get; set; }
    }

    public class RequestDTO<T>
    {
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public T PostObject { get; set; }
        public List<string> Fields { get; set; }
        public SortDTO SortDTO { get; set; }
        public List<SearchDTO> Searching { get; set; }
    }
}