using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class LoyaltyService : ILoyaltyService
{
    private readonly IRepository<TblUser> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecretConfigurationService _secretConfig;

    public LoyaltyService(IRepository<TblUser> userRepository, IUnitOfWork unitOfWork, ISecretConfigurationService secretConfig)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _secretConfig = secretConfig;
    }

    public async Task<int> CalculatePointsForOrderAsync(decimal orderAmount)
    {
        // Reward points based on currency spend
        var pointsPerCurrencyStr = await _secretConfig.GetSecretAsync("LOYALTY_POINTS_PER_CURRENCY") ?? "10000";
        if (decimal.TryParse(pointsPerCurrencyStr, out var pointsPerCurrency) && pointsPerCurrency > 0)
        {
            return (int)(orderAmount / pointsPerCurrency);
        }
        return (int)(orderAmount / 10000); // Fallback
    }

    public async Task AddPointsToUserAsync(string userCode, int points)
    {
        var user = await _userRepository.GetByCodeAsync(userCode, default);
        if (user != null)
        {
            user.AddLoyaltyPoints(points);
            
            // Calculate User Tier based on points or total spend (Simplified: points * 10k)
            // Points are earned 1 per 10k, so 1000 points = 10,000,000 VND
            var currentPoints = user.LoyaltyPoints;
            var newTier = "NEW";
            
            var vipThresholdStr = await _secretConfig.GetSecretAsync("LOYALTY_TIER_VIP_THRESHOLD") ?? "5000";
            var loyalThresholdStr = await _secretConfig.GetSecretAsync("LOYALTY_TIER_LOYAL_THRESHOLD") ?? "1000";
            
            int.TryParse(vipThresholdStr, out var vipThreshold);
            int.TryParse(loyalThresholdStr, out var loyalThreshold);

            if (currentPoints >= vipThreshold) newTier = "VIP";
            else if (currentPoints >= loyalThreshold) newTier = "LOYAL";

            // Need to add UpdateTier method to TblUser or use reflection since it's private set
            var tierProp = typeof(TblUser).GetProperty("UserTier");
            tierProp?.SetValue(user, newTier);

            _userRepository.Update(user);
            await _unitOfWork.CommitAsync();
        }
    }

    public async Task<Result<int>> GetUserPointsAsync(string userCode)
    {
        var user = await _userRepository.GetByCodeAsync(userCode, default);
        if (user == null) return Result.Failure<int>("User not found");
        return Result.Success(user.LoyaltyPoints);
    }
}
