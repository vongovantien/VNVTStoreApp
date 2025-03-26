using vnvt_back_end.Core.General;

namespace vnvt_back_end.Core.DAL.DTO
{
    public class SearchDTO
    {
        public string SearchField { get; set; }
        public SearchCondition SearchCondition { get; set; }
        public dynamic SearchValue { get; set; }
        public string CombineCondition { get; set; }
        public short? GroupID { get; set; }

        public SearchDTO() { }
        public SearchDTO(string searchField, SearchCondition searchCondition, dynamic searchValue,
            string combineCondition = null, short? groupID = null)
        {
            SearchField = searchField;
            SearchCondition = searchCondition;
            SearchValue = searchValue;
            CombineCondition = combineCondition;
            GroupID = groupID;
        }
    }
}