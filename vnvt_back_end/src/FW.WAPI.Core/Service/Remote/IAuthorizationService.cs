using System.Threading.Tasks;

namespace FW.WAPI.Core.Service.Remote
{
    public interface IAuthorizationService
    {
        Task<bool> CheckAuthorization(string[] roles, string funtioncName, string permissionName);
        Task<bool> CheckAuthorization(string[] roles, string[] funtioncNames, string[] permissionNames);
    }
}
