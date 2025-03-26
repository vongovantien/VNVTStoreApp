namespace FW.WAPI.Core.MultiTenancy
{
    public class WebMultiTenancyConfiguration : IWebMultiTenancyConfiguration
    {
        public string DomainFormat { get; set; } = "http://{TENANCY_NAME}.mysite.com";
    }
}