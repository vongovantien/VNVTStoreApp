using System.Threading.Tasks;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Interfaces;

public interface ILoyaltyService
{
    Task<int> CalculatePointsForOrderAsync(decimal orderAmount);
    Task AddPointsToUserAsync(string userCode, int points);
    Task<Result<int>> GetUserPointsAsync(string userCode);
}
