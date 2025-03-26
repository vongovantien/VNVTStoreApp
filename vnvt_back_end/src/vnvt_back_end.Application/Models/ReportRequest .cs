using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vnvt_back_end.Application.Models
{
    public class ReportRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? ProductId { get; set; }
        public string Category { get; set; }
        public string Customer { get; set; }
    }
}
