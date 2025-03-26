using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vnvt_back_end.Application.Models
{
    public class ProductFilter : PagingParameters
    {
        public int? CategoryId { get; set; }
    }
}
