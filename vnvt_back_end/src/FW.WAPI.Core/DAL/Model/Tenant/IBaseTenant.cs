namespace FW.WAPI.Core.DAL.Model.Tenant
{
    public interface IBaseTenant
    {
        string Code { get; set; }
        string Name { get; set; }
        bool IsActive { get; set; }
        string ConnectionString { get; set; }
    }
}