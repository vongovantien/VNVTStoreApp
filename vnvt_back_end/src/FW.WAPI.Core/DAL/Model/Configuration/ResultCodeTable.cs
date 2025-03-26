namespace FW.WAPI.Core.DAL.Model.Configuration
{
    public class ResultCodeTable
    {
        public ResultCode Ok { get; set; }
        public ResultCode NullParams { get; set; }
        public ResultCode InvUser { get; set; }
        public ResultCode BadRequest { get; set; }
        public ResultCode NotFoundToken { get; set; }
        public ResultCode TokenExpired { get; set; }
        public ResultCode AddDbFail { get; set; }
        public ResultCode UpdateDbFail { get; set; }
        public ResultCode DuplicateKey { get; set; }
        public ResultCode DeleteDbFail { get; set; }
        public ResultCode ExpireTokenFail { get; set; }
        public ResultCode HandleRequestFail { get; set; }
        public ResultCode RefreshTokenExpired { get; set; }
        public ResultCode Friendly { get; set; }
        public ResultCode ConncurrencyUpdate { get; set; }
        public ResultCode Confirmation { get; set; }
        public ResultCode Warning { get; set; }
    }
}