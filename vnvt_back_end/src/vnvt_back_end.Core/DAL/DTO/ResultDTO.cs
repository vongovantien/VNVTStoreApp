using System.Collections.Generic;

namespace vnvt_back_end.Core.DAL.DTO
{
    public class ResultDTO<TEntity>
    {
        public IEnumerable<TEntity> Data { get; set; }

        public long Total { get; set; }
    }
}