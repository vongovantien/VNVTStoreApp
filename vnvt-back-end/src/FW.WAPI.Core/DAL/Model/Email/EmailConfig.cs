

namespace FW.WAPI.Core.DAL.Model.Email
{
    public class EmailConfig
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Body { get; set; }
        public string[] To { get; set; }
        public string Subject { get; set; }

        public EmailConfig()
        {

        }
    }
}
