
namespace FW.WAPI.Core.DAL.Model.Authentication
{
    public class AuthenticationDTO
    {
        public string grant_type { get; set; }
        public string refresh_token { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string guid { get; set; }
        public string tenant { get; set; }
        public string token { get; set; }
        public string deviceId { get; set; }
    }
}
