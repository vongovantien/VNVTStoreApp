namespace FW.WAPI.Core.MultiTenancy
{
    public interface IWebMultiTenancyConfiguration
    {
        string DomainFormat { get; set; }
    }
}