using FW.WAPI.Core.DAL.DTO;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Domain.Context
{
    public interface IGenerateUniqueCodeAsync
    {
        Task<AutoSettingValue> GetUniqueCodePrefixAsync(string tableName, string companyCode = null);
    }
}
