
namespace FW.WAPI.Core.Domain.Context
{
    public interface IGenerateUniqueCode
    {
        string GetUniqueCodePrefix(string tableName, ref int maxLength, string companyCode = null);
      
    }
}