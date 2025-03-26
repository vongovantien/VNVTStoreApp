namespace FW.WAPI.Core.DAL.DTO
{
    public class ResponseDTO
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
        public long Total { get; set; }
    }
}