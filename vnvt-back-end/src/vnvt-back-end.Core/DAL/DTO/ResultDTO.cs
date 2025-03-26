using System.Collections.Generic;

namespace FW.WAPI.Core.DAL.DTO
{
    public class ResultDTO<TEntity>
    {
        public IEnumerable<TEntity> Data { get; set; }

        public long Total { get; set; }
    }
}