using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Infrastructure.Services;

public class LoyaltyService : ILoyaltyService
{
    private readonly IRepository<TblUser> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoyaltyService(IRepository<TblUser> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<int> CalculatePointsForOrderAsync(decimal orderAmount)
    {
        // Reward 1 point for every 10,000 VND spent
        return Task.FromResult((int)(orderAmount / 10000));
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
            if (currentPoints >= 5000) newTier = "VIP"; // > 50M
            else if (currentPoints >= 1000) newTier = "LOYAL"; // > 10M

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
