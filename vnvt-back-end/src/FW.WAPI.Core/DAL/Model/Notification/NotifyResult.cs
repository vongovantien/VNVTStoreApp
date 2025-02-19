using System;
using System.Collections.Generic;
using System.Text;

namespace FW.WAPI.Core.DAL.DTO
{
    public class NotifyResult
    {
        public bool Status { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public string ResultId { get; set; }
        public object Data { get; set; }
        public bool IsIgnoreError { get; set; }
        public object OriginResult { get; set; }
    }
}
